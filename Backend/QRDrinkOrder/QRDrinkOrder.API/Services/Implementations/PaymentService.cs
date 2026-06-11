using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QRDrinkOrder.API.Hubs;
using QRDrinkOrder.API.Services.Interfaces;
using QRDrinkOrder.Shared.DTOs.Responses;
using QRDrinkOrder.Shared.Enums;
using QRDrinkOrder.API.Models;
using System.Text.RegularExpressions;

namespace QRDrinkOrder.API.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly QrdrinkOrderDbContext _context;
    private readonly IHubContext<OrderHub> _hubContext;

    public PaymentService(QrdrinkOrderDbContext context, IHubContext<OrderHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<bool> ProcessSePayWebhookAsync(string content, decimal amount, string transactionId, string accountNumber)
    {
        // 1. Kiểm tra AccountNumber có hợp lệ và được kích hoạt không
        var validAccount = await _context.BankAccounts
            .FirstOrDefaultAsync(b => b.AccountNumber == accountNumber && b.IsActive);
            
        if (validAccount == null)
        {
            Console.WriteLine($"[SePay Webhook] Bỏ qua - Tài khoản {accountNumber} không hợp lệ hoặc chưa được kích hoạt.");
            return false;
        }

        // 2. Phân tích nội dung chuyển khoản tìm Order ID (ví dụ: UwU15)
        var match = Regex.Match(content, @"(?:UwU)(\d+)", RegexOptions.IgnoreCase);
        if (!match.Success)
            return false;

        if (!int.TryParse(match.Groups[1].Value, out int orderId))
            return false;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = await _context.Orders
                .Include(o => o.Payment)
                .Include(o => o.Session)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Drink)
                        .ThenInclude(d => d.DrinkTranslations)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null || order.Payment == null)
                return false;

            // Nếu đơn hàng đã hoàn tất thanh toán trước đó
            if (order.Payment.PaymentStatus == (byte)PaymentStatus.Success)
                return true;

            // Kiểm tra số tiền chuyển khoản
            if (amount < order.Payment.Amount)
            {
                order.Payment.PaymentStatus = (byte)PaymentStatus.Failed;
                order.Payment.TransactionId = transactionId;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Báo lỗi cho khách hàng và nhân viên
                await _hubContext.Clients.Group($"Customer_{order.SessionId}").SendAsync("ReceiveStatusUpdate", new
                {
                    OrderId = order.OrderId,
                    Status = order.OrderStatus,
                    StatusName = "Thanh toán thất bại (Thiếu tiền)"
                });
                return false;
            }

            // Thanh toán thành công
            order.Payment.PaymentStatus = (byte)PaymentStatus.Success;
            order.Payment.TransactionId = transactionId;
            order.Payment.PaidAt = DateTime.Now;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Ánh xạ sang DTO để gửi SignalR
            var orderDto = MapToOrderDto(order);

            // Thông báo thời gian thực qua SignalR
            // 1. Gửi đơn đã thanh toán thành công lên màn hình Kanban của bếp
            await _hubContext.Clients.Group("Staff").SendAsync("ReceiveNewOrder", orderDto);

            // 2. Gửi cập nhật trạng thái đơn cho thiết bị khách hàng (Vẫn ở 0 nhưng đã thanh toán)
            await _hubContext.Clients.Group($"Customer_{order.SessionId}").SendAsync("ReceiveStatusUpdate", new
            {
                OrderId = order.OrderId,
                Status = order.OrderStatus,
                StatusName = "Đã thanh toán"
            });

            // 3. Đồng bộ trạng thái đơn ở POS
            await _hubContext.Clients.Group("Staff").SendAsync("ReceiveStatusUpdateAtPOS", new
            {
                OrderId = order.OrderId,
                Status = order.OrderStatus,
                StatusName = "Đã thanh toán",
                PaymentStatus = (byte)1
            });

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> ConfirmCashPaymentAsync(int orderId, int employeeId)
    {
        var order = await _context.Orders
            .Include(o => o.Payment)
            .Include(o => o.Session)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Drink)
                    .ThenInclude(d => d.DrinkTranslations)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null || order.Payment == null)
            return false;

        // Chỉ xác nhận khi phương thức thanh toán là Tiền mặt
        if (order.Payment.PaymentMethod != (byte)PaymentMethod.Cash)
            return false;

        order.Payment.PaymentStatus = (byte)PaymentStatus.Success;
        order.Payment.PaidAt = DateTime.Now;
        order.Payment.TransactionId = $"CASH_CONFIRMED_BY_EMP_{employeeId}";

        // Không tự động chuyển sang Đang chuẩn bị nữa, để nhân viên tự bấm Nhận đơn
        order.EmployeeId = employeeId;

        await _context.SaveChangesAsync();

        var orderDto = MapToOrderDto(order);

        // Phát tín hiệu SignalR đồng bộ các màn hình
        await _hubContext.Clients.Group($"Customer_{order.SessionId}").SendAsync("ReceiveStatusUpdate", new
        {
            OrderId = order.OrderId,
            Status = order.OrderStatus,
            StatusName = "Đã thanh toán",
            PaymentStatus = (byte)1
        });

        await _hubContext.Clients.Group("Staff").SendAsync("ReceiveStatusUpdateAtPOS", new
        {
            OrderId = order.OrderId,
            Status = order.OrderStatus,
            StatusName = "Đã thanh toán",
            PaymentStatus = (byte)1
        });

        return true;
    }

    private static string GetStatusName(byte status)
    {
        return status switch
        {
            (byte)OrderStatus.PendingPayment => "Chờ thanh toán",
            (byte)OrderStatus.Preparing => "Đang chuẩn bị",
            (byte)OrderStatus.Completed => "Hoàn thành",
            (byte)OrderStatus.Cancelled => "Đã hủy",
            _ => "Không xác định"
        };
    }

    private static string GetPaymentMethodName(byte method)
    {
        return method switch
        {
            (byte)PaymentMethod.Cash => "Tiền mặt",
            (byte)PaymentMethod.SePay => "SePay VietQR",
            _ => "Không xác định"
        };
    }

    private static string GetPaymentStatusName(byte status)
    {
        return status switch
        {
            (byte)PaymentStatus.Pending => "Đang chờ",
            (byte)PaymentStatus.Success => "Thành công",
            (byte)PaymentStatus.Failed => "Thất bại",
            _ => "Không xác định"
        };
    }

    private static OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            OrderId = order.OrderId,
            SessionId = order.SessionId,
            EmployeeId = order.EmployeeId,
            EmployeeName = order.Employee?.FullName,
            TableNumber = order.TableNumber,
            TotalAmount = order.TotalAmount,
            DiscountAmount = order.DiscountAmount ?? 0,
            FinalAmount = order.FinalAmount ?? (order.TotalAmount - (order.DiscountAmount ?? 0)),
            OrderStatus = order.OrderStatus ?? 0,
            OrderStatusName = GetStatusName(order.OrderStatus ?? 0),
            CouponId = order.CouponId,
            OrderDate = order.OrderDate ?? DateTime.Now,
            Note = order.Note,
            CustomerPhone = order.Session?.Phone,
            Items = order.OrderItems.Select(oi => new OrderItemDto
            {
                OrderItemId = oi.OrderItemId,
                OrderId = oi.OrderId,
                DrinkId = oi.DrinkId,
                DrinkName = oi.Drink?.DrinkTranslations.FirstOrDefault(t => t.LanguageCode.Trim() == "vi")?.DrinkName ?? "Sản phẩm",
                ImageUrl = oi.Drink?.ImageUrl ?? "",
                Quantity = oi.Quantity,
                SweetnessLevel = oi.SweetnessLevel,
                IceLevel = oi.IceLevel,
                ItemNote = oi.ItemNote,
                UnitPrice = oi.UnitPrice,
                SubTotal = oi.SubTotal ?? (oi.Quantity * oi.UnitPrice)
            }).ToList(),
            Payment = order.Payment == null ? null : new PaymentDto
            {
                PaymentId = order.Payment.PaymentId,
                OrderId = order.Payment.OrderId,
                PaymentMethod = order.Payment.PaymentMethod,
                PaymentMethodName = GetPaymentMethodName(order.Payment.PaymentMethod),
                PaymentStatus = order.Payment.PaymentStatus ?? 0,
                PaymentStatusName = GetPaymentStatusName(order.Payment.PaymentStatus ?? 0),
                TransactionId = order.Payment.TransactionId,
                Amount = order.Payment.Amount,
                PaidAt = order.Payment.PaidAt
            }
        };
    }
}
