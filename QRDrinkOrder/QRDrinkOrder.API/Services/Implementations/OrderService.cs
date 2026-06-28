using QRDrinkOrder.Shared.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QRDrinkOrder.API.Hubs;
using QRDrinkOrder.API.Services.Interfaces;
using QRDrinkOrder.Shared.Constants;
using QRDrinkOrder.Shared.DTOs.Requests;
using QRDrinkOrder.Shared.DTOs.Responses;
using QRDrinkOrder.Shared.Enums;
using QRDrinkOrder.API.Models;

namespace QRDrinkOrder.API.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly QrdrinkOrderDbContext _context;
    private readonly IHubContext<OrderHub> _hubContext;
    private readonly INotificationService _notificationService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(QrdrinkOrderDbContext context, IHubContext<OrderHub> hubContext, INotificationService notificationService, ILogger<OrderService> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<OrderDto> CreateOrderAsync(Guid sessionId, CreateOrderRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Quản lý session
            var session = await _context.CustomerSessions.FindAsync(sessionId);
            if (session == null)
            {
                session = new CustomerSession
                {
                    SessionId = sessionId,
                    Phone = request.Phone,
                    PreferredLanguage = "vi",
                    CreatedAt = DateTime.Now
                };
                _context.CustomerSessions.Add(session);
            }
            else if (!string.IsNullOrEmpty(request.Phone) && string.IsNullOrEmpty(session.Phone))
            {
                session.Phone = request.Phone;
            }

            await _context.SaveChangesAsync();

            // 2. Tính tiền sản phẩm
            var drinkIds = request.Items.Select(i => i.DrinkId).ToList();
            var drinks = await _context.Drinks
                .Include(d => d.DrinkTranslations)
                .Where(d => drinkIds.Contains(d.DrinkId))
                .ToDictionaryAsync(d => d.DrinkId);

            var sizes = await _context.Sizes.ToDictionaryAsync(s => s.SizeId);
            var toppings = await _context.Toppings.ToDictionaryAsync(t => t.ToppingId);

            decimal totalAmount = CalculateTotalAmount(request.Items, drinks, sizes, toppings);

            // 3. Áp dụng Mã giảm giá (nếu có)
            var (discountAmount, coupon, isCouponApplied) = await CalculateCouponDiscountAsync(request.Phone, request.CouponCode, totalAmount);

            // 4. Áp dụng ưu đãi nhân viên (nếu nhân viên gọi món hộ và chưa dùng mã giảm giá khác)
            bool isEmployeeBenefitApplied = false;
            if (!isCouponApplied && request.EmployeeId.HasValue && request.UseEmployeeBenefit)
            {
                var employee = await _context.Employees.FindAsync(request.EmployeeId.Value);
                if (employee != null)
                {
                    var today = DateOnly.FromDateTime(DateTime.Today);
                    var hasUsedBenefit = await _context.StaffBenefits.AnyAsync(sb => sb.EmployeeId == employee.EmployeeId && sb.UsedDate == today);
                    if (!hasUsedBenefit)
                    {
                        // Giảm 50% cho đơn hàng đầu tiên trong ngày của nhân viên
                        discountAmount = totalAmount * 0.50m;
                        isEmployeeBenefitApplied = true;
                    }
                    else
                    {
                        throw new BusinessException("Bạn đã sử dụng ưu đãi 50% dành cho nhân viên trong hôm nay.");
                    }
                }
            }

            // 4.5. Áp dụng Điểm thưởng (Nếu khách hàng chọn dùng điểm)
            int pointsUsed = 0;
            if (request.PointsToUse.HasValue && request.PointsToUse.Value > 0)
            {
                if (string.IsNullOrEmpty(request.Phone))
                    throw new BusinessException("Yêu cầu nhập số điện thoại để sử dụng điểm.");

                var membership = await _context.Memberships.FirstOrDefaultAsync(m => m.Phone == request.Phone);
                if (membership == null || membership.Points < request.PointsToUse.Value)
                    throw new BusinessException("Điểm tích lũy không đủ để sử dụng.");

                var pointRateConfig = await _context.SystemConfigs.FindAsync("RedeemPointRate");
                int redeemRate = pointRateConfig != null ? int.Parse(pointRateConfig.ConfigValue) : 1000;

                decimal pointDiscount = request.PointsToUse.Value * redeemRate;
                
                if (discountAmount + pointDiscount > totalAmount)
                {
                    throw new BusinessException("Số điểm sử dụng vượt quá giá trị đơn hàng.");
                }

                discountAmount += pointDiscount;
                pointsUsed = request.PointsToUse.Value;
                
                // Trừ điểm ngay lập tức
                membership.Points -= pointsUsed;
                _context.PointHistories.Add(new PointHistory
                {
                    Phone = membership.Phone,
                    PointsChanged = -pointsUsed,
                    Reason = "Dùng điểm thanh toán đơn hàng"
                });
            }

            // 5. Tạo đơn hàng mới
            var order = new Order
            {
                SessionId = session.SessionId,
                EmployeeId = request.EmployeeId,
                TableNumber = request.TableNumber,
                TotalAmount = totalAmount,
                DiscountAmount = discountAmount,
                PointsUsed = pointsUsed > 0 ? pointsUsed : null,
                OrderStatus = (byte)OrderStatus.PendingPayment,
                CouponId = coupon?.CouponId,
                OrderDate = DateTime.Now,
                Note = request.Note
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 6. Thêm chi tiết đơn hàng
            foreach (var item in request.Items)
            {
                var drink = drinks[item.DrinkId];
                decimal unitPrice = drink.BasePrice;
                if (item.SizeId.HasValue && sizes.TryGetValue(item.SizeId.Value, out var size)) unitPrice += size.PriceOffset;
                
                if (item.ToppingIds != null)
                {
                    foreach (var tId in item.ToppingIds) if (toppings.TryGetValue(tId, out var topping)) unitPrice += topping.Price;
                }

                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    DrinkId = item.DrinkId,
                    Quantity = item.Quantity,
                    SweetnessLevel = item.SweetnessLevel,
                    IceLevel = item.IceLevel,
                    ItemNote = item.ItemNote,
                    SizeId = item.SizeId,
                    UnitPrice = unitPrice
                };

                if (item.ToppingIds != null)
                {
                    foreach (var tId in item.ToppingIds)
                    {
                        if (toppings.ContainsKey(tId))
                        {
                            orderItem.OrderItemToppings.Add(new OrderItemTopping { ToppingId = tId });
                        }
                    }
                }

                _context.OrderItems.Add(orderItem);
            }

            // 7. Tạo hóa đơn thanh toán
            var payment = new Payment
            {
                OrderId = order.OrderId,
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = (byte)PaymentStatus.Pending,
                Amount = totalAmount - discountAmount
            };
            _context.Payments.Add(payment);

            // 8. Lưu vết lịch sử và tăng bộ đếm mã giảm giá
            if (isCouponApplied && coupon != null)
            {
                var usage = new CouponUsage
                {
                    CouponId = coupon.CouponId,
                    Phone = request.Phone!,
                    OrderId = order.OrderId,
                    UsedAt = DateTime.Now
                };
                _context.CouponUsages.Add(usage);
                coupon.UsedCount += 1;
            }

            // Ghi nhận phúc lợi nhân viên
            if (isEmployeeBenefitApplied && request.EmployeeId.HasValue)
            {
                var benefit = new StaffBenefit
                {
                    EmployeeId = request.EmployeeId.Value,
                    OrderId = order.OrderId,
                    UsedDate = DateOnly.FromDateTime(DateTime.Today)
                };
                _context.StaffBenefits.Add(benefit);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var result = await GetOrderByIdAsync(order.OrderId);

            // Gửi thông báo SignalR báo có đơn mới tới POS cho TẤT CẢ các phương thức thanh toán
            if (result != null)
            {
                await _hubContext.Clients.Group("Staff").SendAsync("ReceiveNewOrder", result);
            }

            return result!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra trong quá trình tạo đơn hàng");
            await transaction.RollbackAsync();
            throw;
        }
    }

    private IQueryable<Order> IncludeOrderDetails(IQueryable<Order> query)
    {
        return query
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Drink)
                    .ThenInclude(d => d.DrinkTranslations)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Size)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemToppings)
                    .ThenInclude(oit => oit.Topping)
            .Include(o => o.Payment)
            .Include(o => o.Employee)
            .Include(o => o.Session)
            .Include(o => o.Review)
            .Include(o => o.Coupon);
    }

    public async Task<List<OrderDto>> GetActiveOrdersAsync()
    {
        var query = IncludeOrderDetails(_context.Orders.AsNoTracking().AsSplitQuery());
        var orders = await query
            .Where(o => o.OrderStatus != (byte)OrderStatus.Completed && o.OrderStatus != (byte)OrderStatus.Cancelled)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return orders.Select(MapToOrderDto).ToList();
    }

    public async Task<List<OrderDto>> GetOrderHistoryByPhoneAsync(string phone)
    {
        var query = IncludeOrderDetails(_context.Orders.AsNoTracking().AsSplitQuery());
        var orders = await query
            .Where(o => o.Session.Phone == phone)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return orders.Select(MapToOrderDto).ToList();
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
    {
        var query = IncludeOrderDetails(_context.Orders.AsNoTracking().AsSplitQuery());
        var order = await query.FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
            return null;

        return MapToOrderDto(order);
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, byte status, int? employeeId = null)
    {
        var order = await _context.Orders
            .Include(o => o.Payment)
            .Include(o => o.Session)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
            return false;

        order.OrderStatus = status;
        if (employeeId.HasValue)
        {
            order.EmployeeId = employeeId;
        }

        // Nếu trạng thái là Đã hủy hoặc Hoàn thành, cập nhật trạng thái thanh toán tương ứng
        if (status == (byte)OrderStatus.Cancelled && order.Payment != null)
        {
            order.Payment.PaymentStatus = (byte)PaymentStatus.Failed;
        }
        else if (status == (byte)OrderStatus.Completed && order.Payment != null && order.Payment.PaymentMethod == (byte)PaymentMethod.Cash)
        {
            // Đối với tiền mặt, khi hoàn thành đơn nghĩa là đã thanh toán tại quầy
            order.Payment.PaymentStatus = (byte)PaymentStatus.Success;
            order.Payment.PaidAt = DateTime.Now;
        }

        // Tích điểm khi đơn hàng hoàn thành
        if (status == (byte)OrderStatus.Completed && order.Session != null && !string.IsNullOrEmpty(order.Session.Phone))
        {
            var membership = await _context.Memberships.FirstOrDefaultAsync(m => m.Phone == order.Session.Phone);
            if (membership == null)
            {
                membership = new Membership { Phone = order.Session.Phone, Points = 0 };
                _context.Memberships.Add(membership);
            }

            var earnRateConfig = await _context.SystemConfigs.FindAsync("EarnPointRate");
            int earnRate = earnRateConfig != null ? int.Parse(earnRateConfig.ConfigValue) : 10000;
            
            int earnedPoints = (int)((order.FinalAmount ?? 0) / earnRate);
            if (earnedPoints > 0)
            {
                membership.Points += earnedPoints;
                _context.PointHistories.Add(new PointHistory
                {
                    Phone = membership.Phone,
                    PointsChanged = earnedPoints,
                    Reason = $"Tích điểm từ đơn hàng #{order.OrderId}"
                });
            }
        }

        await _context.SaveChangesAsync();

        // Phát tín hiệu đồng bộ SignalR cập nhật trạng thái đơn
        string statusName = GetStatusName(status);
        await _hubContext.Clients.Group($"Customer_{order.SessionId}").SendAsync("ReceiveStatusUpdate", new { OrderId = orderId, Status = status, StatusName = statusName, PaymentStatus = order.Payment?.PaymentStatus });
        await _hubContext.Clients.Group("Staff").SendAsync("ReceiveStatusUpdateAtPOS", new { OrderId = orderId, Status = status, StatusName = statusName, PaymentStatus = order.Payment?.PaymentStatus });

        // Gửi WebPush Notification cho khách hàng nếu có số điện thoại
        if (order.Session != null && !string.IsNullOrEmpty(order.Session.Phone))
        {
            string message = $"Đơn hàng #{orderId} của bạn đã được cập nhật thành: {statusName}.";
            if (status == (byte)OrderStatus.Completed)
            {
                message = $"Đồ uống của bạn đã sẵn sàng tại quầy (Đơn #{orderId}). Chúc bạn ngon miệng!";
            }
            await _notificationService.SendNotificationToPhoneAsync(order.Session.Phone, "Cập nhật đơn hàng", message, $"/track-order/{orderId}");
        }

        return true;
    }

    public async Task<bool> CancelOrderAsync(int orderId, Guid sessionId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = await _context.Orders
                .Include(o => o.Payment)
                .Include(o => o.Coupon)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return false;

            // Ràng buộc bảo mật: Đúng SessionId mới được hủy
            if (order.SessionId != sessionId)
                throw new BusinessException(ErrorMessages.Unauthorized);

            // Ràng buộc F&B: Chỉ được hủy khi chưa pha chế (OrderStatus == 0: Chờ thanh toán)
            if (order.OrderStatus != (byte)OrderStatus.PendingPayment)
                throw new BusinessException(ErrorMessages.OrderCannotCancel);

            order.OrderStatus = (byte)OrderStatus.Cancelled;

            if (order.Payment != null)
            {
                order.Payment.PaymentStatus = (byte)PaymentStatus.Failed;
            }

            // Hoàn lại mã giảm giá
            if (order.CouponId.HasValue)
            {
                var session = await _context.CustomerSessions.FindAsync(sessionId);
                if (session != null && !string.IsNullOrEmpty(session.Phone))
                {
                    var usage = await _context.CouponUsages.FirstOrDefaultAsync(cu => cu.CouponId == order.CouponId.Value && cu.Phone == session.Phone);
                    if (usage != null)
                    {
                        _context.CouponUsages.Remove(usage);
                    }
                }

                if (order.Coupon != null)
                {
                    order.Coupon.UsedCount = Math.Max(0, order.Coupon.UsedCount.GetValueOrDefault() - 1);
                }
            }

            // Hoàn lại phúc lợi nhân viên (nếu có)
            if (order.EmployeeId.HasValue)
            {
                var benefit = await _context.StaffBenefits.FirstOrDefaultAsync(sb => sb.OrderId == order.OrderId);
                if (benefit != null)
                {
                    _context.StaffBenefits.Remove(benefit);
                }
            }

            // Hoàn lại điểm (nếu dùng)
            if (order.PointsUsed.HasValue && order.PointsUsed.Value > 0)
            {
                var session = await _context.CustomerSessions.FindAsync(sessionId);
                if (session != null && !string.IsNullOrEmpty(session.Phone))
                {
                    var membership = await _context.Memberships.FirstOrDefaultAsync(m => m.Phone == session.Phone);
                    if (membership != null)
                    {
                        membership.Points += order.PointsUsed.Value;
                        _context.PointHistories.Add(new PointHistory
                        {
                            Phone = membership.Phone,
                            PointsChanged = order.PointsUsed.Value,
                            Reason = $"Hoàn điểm do hủy đơn #{order.OrderId}"
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Phát tín hiệu SignalR hủy đơn
            string statusName = GetStatusName((byte)OrderStatus.Cancelled);
            await _hubContext.Clients.Group($"Customer_{order.SessionId}").SendAsync("ReceiveStatusUpdate", new { OrderId = orderId, Status = (byte)OrderStatus.Cancelled, StatusName = statusName, PaymentStatus = order.Payment?.PaymentStatus });
            await _hubContext.Clients.Group("Staff").SendAsync("ReceiveStatusUpdateAtPOS", new { OrderId = orderId, Status = (byte)OrderStatus.Cancelled, StatusName = statusName, PaymentStatus = order.Payment?.PaymentStatus });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra trong quá trình hủy đơn hàng {OrderId}", orderId);
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdatePaymentMethodAsync(int orderId, Guid sessionId, byte paymentMethod)
    {
        var order = await _context.Orders
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null || order.Payment == null)
            return false;

        // Ràng buộc bảo mật: Đúng SessionId mới được sửa
        if (order.SessionId != sessionId)
            throw new BusinessException(ErrorMessages.Unauthorized);

        // Chỉ được đổi phương thức khi chưa thanh toán
        if (order.Payment.PaymentStatus != (byte)PaymentStatus.Pending)
            throw new BusinessException("Không thể thay đổi phương thức cho đơn hàng đã thanh toán.");

        order.Payment.PaymentMethod = paymentMethod;

        // Cập nhật phương thức trong bảng Payment
        await _context.SaveChangesAsync();

        // Gửi SignalR cho màn hình thu ngân cập nhật phương thức
        await _hubContext.Clients.Group("Staff").SendAsync("ReceiveStatusUpdateAtPOS", new 
        { 
            OrderId = orderId, 
            Status = order.OrderStatus, 
            StatusName = GetStatusName(order.OrderStatus ?? 0), 
            PaymentStatus = order.Payment.PaymentStatus,
            PaymentMethod = paymentMethod
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

    private decimal CalculateTotalAmount(List<OrderItemRequest> items, Dictionary<int, Drink> drinks, Dictionary<int, Size> sizes, Dictionary<int, Topping> toppings)
    {
        decimal totalAmount = 0;
        foreach (var item in items)
        {
            if (!drinks.TryGetValue(item.DrinkId, out var drink))
                throw new BusinessException($"Không tìm thấy sản phẩm với mã {item.DrinkId}.");

            decimal unitPrice = drink.BasePrice;
            if (item.SizeId.HasValue && sizes.TryGetValue(item.SizeId.Value, out var size))
            {
                unitPrice += size.PriceOffset;
            }

            if (item.ToppingIds != null)
            {
                foreach (var tId in item.ToppingIds)
                {
                    if (toppings.TryGetValue(tId, out var topping))
                    {
                        unitPrice += topping.Price;
                    }
                }
            }

            totalAmount += item.Quantity * unitPrice;
        }
        return totalAmount;
    }

    private async Task<(decimal discountAmount, Coupon? coupon, bool isApplied)> CalculateCouponDiscountAsync(string? phone, string? couponCode, decimal totalAmount)
    {
        if (string.IsNullOrEmpty(couponCode)) return (0, null, false);
        if (string.IsNullOrEmpty(phone)) throw new BusinessException("Yêu cầu nhập số điện thoại để áp dụng mã giảm giá.");

        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.CouponCode == couponCode && c.IsActive == true);
        if (coupon == null || DateTime.Now < coupon.StartDate || DateTime.Now > coupon.EndDate)
            throw new BusinessException(ErrorMessages.InvalidCoupon);

        if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
            throw new BusinessException(ErrorMessages.CouponLimitReached);

        if (totalAmount < coupon.MinOrderValue)
            throw new BusinessException(ErrorMessages.MinOrderNotMet);

        // Chặn lạm dụng mã theo SĐT
        var hasUsed = await _context.CouponUsages.AnyAsync(cu => cu.CouponId == coupon.CouponId && cu.Phone == phone);
        if (hasUsed)
            throw new BusinessException(ErrorMessages.CouponAlreadyUsed);

        // Tính toán số tiền giảm
        decimal discountAmount = 0;
        if (coupon.DiscountType == (byte)DiscountType.Fixed)
        {
            discountAmount = coupon.DiscountValue;
        }
        else if (coupon.DiscountType == (byte)DiscountType.Percentage)
        {
            discountAmount = totalAmount * (coupon.DiscountValue / 100m);
            if (coupon.MaxDiscountAmount.HasValue)
            {
                discountAmount = Math.Min(discountAmount, coupon.MaxDiscountAmount.Value);
            }
        }

        discountAmount = Math.Min(discountAmount, totalAmount);
        return (discountAmount, coupon, true);
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
            CouponCode = order.Coupon?.CouponCode,
            PointsUsed = order.PointsUsed,
            OrderDate = order.OrderDate ?? DateTime.Now,
            Note = order.Note,
            CustomerPhone = order.Session?.Phone,
            IsReviewed = order.Review != null,
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
                SizeName = oi.Size?.Name,
                ToppingNames = oi.OrderItemToppings.Select(oit => oit.Topping.Name).ToList(),
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

    public async Task<List<OrderDto>> GetAllOrdersAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = IncludeOrderDetails(_context.Orders.AsNoTracking().AsSplitQuery());

        if (startDate.HasValue)
        {
            var start = startDate.Value.Date;
            query = query.Where(o => o.OrderDate >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(o => o.OrderDate <= end);
        }

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return orders.Select(MapToOrderDto).ToList();
    }
}

