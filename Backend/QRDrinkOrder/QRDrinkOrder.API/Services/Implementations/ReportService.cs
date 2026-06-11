using Microsoft.EntityFrameworkCore;
using QRDrinkOrder.API.Services.Interfaces;
using QRDrinkOrder.Shared.DTOs.Responses;
using QRDrinkOrder.Shared.Enums;
using QRDrinkOrder.API.Models;

namespace QRDrinkOrder.API.Services.Implementations;

public class ReportService : IReportService
{
    private readonly QrdrinkOrderDbContext _context;

    public ReportService(QrdrinkOrderDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDto> GetDashboardDataAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var end = endDate ?? DateTime.Now;
        var start = startDate ?? end.AddDays(-30);

        // Đảm bảo thời gian bắt đầu từ 00:00:00 và kết thúc ở 23:59:59
        start = start.Date;
        end = end.Date.AddDays(1).AddTicks(-1);

        // 1. Lấy danh sách các đơn hàng trong khoảng thời gian
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Payment)
            .Include(o => o.Coupon)
            .AsSplitQuery()
            .Where(o => o.OrderDate >= start && o.OrderDate <= end)
            .ToListAsync();

        var completedOrders = orders.Where(o => o.OrderStatus == (byte)OrderStatus.Completed).ToList();

        // 2. Tính toán Overview Metrics
        decimal totalRevenue = completedOrders.Sum(o => o.FinalAmount ?? (o.TotalAmount - (o.DiscountAmount ?? 0)));
        int totalOrders = orders.Count;
        int completedCount = completedOrders.Count;
        int cancelledCount = orders.Count(o => o.OrderStatus == (byte)OrderStatus.Cancelled);

        decimal cashRevenue = completedOrders
            .Where(o => o.Payment?.PaymentMethod == (byte)PaymentMethod.Cash)
            .Sum(o => o.FinalAmount ?? (o.TotalAmount - (o.DiscountAmount ?? 0)));

        decimal sePayRevenue = completedOrders
            .Where(o => o.Payment?.PaymentMethod == (byte)PaymentMethod.SePay)
            .Sum(o => o.FinalAmount ?? (o.TotalAmount - (o.DiscountAmount ?? 0)));

        int activeCustomers = await _context.CustomerSessions
            .Where(s => s.CreatedAt >= start && s.CreatedAt <= end && !string.IsNullOrEmpty(s.Phone))
            .Select(s => s.Phone)
            .Distinct()
            .CountAsync();

        if (activeCustomers == 0)
        {
            activeCustomers = orders.Select(o => o.SessionId).Distinct().Count();
        }

        int activeCoupons = await _context.Coupons
            .CountAsync(c => c.IsActive == true && c.EndDate >= DateTime.Now);

        var overview = new DashboardOverviewDto
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            CompletedOrders = completedCount,
            CancelledOrders = cancelledCount,
            ActiveCustomersCount = activeCustomers,
            ActiveCouponsCount = activeCoupons,
            CashRevenue = cashRevenue,
            SePayRevenue = sePayRevenue
        };

        // 3. Daily Revenue (Thống kê doanh thu theo ngày)
        var dailyQuery = completedOrders
            .GroupBy(o => o.OrderDate ?? DateTime.Now)
            .Select(g => new DailyRevenueDto
            {
                DateStr = g.Key.ToString("dd/MM"),
                Revenue = g.Sum(o => o.FinalAmount ?? (o.TotalAmount - (o.DiscountAmount ?? 0))),
                OrderCount = g.Count()
            })
            .ToList();

        // Gom nhóm lại theo ngày (vì GroupBy DateTime chứa cả giờ phút giây nên có thể lệch)
        var dailyRevenue = dailyQuery
            .GroupBy(d => d.DateStr)
            .Select(g => new DailyRevenueDto
            {
                DateStr = g.Key,
                Revenue = g.Sum(x => x.Revenue),
                OrderCount = g.Sum(x => x.OrderCount)
            })
            .OrderBy(d => d.DateStr)
            .ToList();

        // 4. Peak Hours (Thống kê khung giờ cao điểm)
        var peakHours = completedOrders
            .GroupBy(o => (o.OrderDate ?? DateTime.Now).Hour)
            .Select(g => new PeakHourDto
            {
                Hour = g.Key,
                Revenue = g.Sum(o => o.FinalAmount ?? (o.TotalAmount - (o.DiscountAmount ?? 0))),
                OrderCount = g.Count()
            })
            .OrderBy(p => p.Hour)
            .ToList();

        // Đảm bảo đầy đủ 24h trong danh sách nếu cần thiết, ở đây ta cứ lấy theo dữ liệu thực tế có

        // 5. Popular Drinks (Món bán chạy nhất)
        var orderIds = completedOrders.Select(o => o.OrderId).ToList();

        var popularDrinks = await _context.OrderItems
            .AsNoTracking()
            .Include(oi => oi.Drink)
                .ThenInclude(d => d.DrinkTranslations)
            .AsSplitQuery()
            .Where(oi => orderIds.Contains(oi.OrderId))
            .GroupBy(oi => oi.DrinkId)
            .Select(g => new PopularDrinkDto
            {
                DrinkId = g.Key,
                DrinkName = g.First().Drink.DrinkTranslations.FirstOrDefault(t => t.LanguageCode.Trim() == "vi")!.DrinkName,
                ImageUrl = g.First().Drink.ImageUrl,
                QuantitySold = g.Sum(oi => oi.Quantity),
                RevenueGenerated = g.Sum(oi => oi.SubTotal ?? (oi.Quantity * oi.UnitPrice))
            })
            .OrderByDescending(d => d.QuantitySold)
            .Take(5)
            .ToListAsync();

        // 6. Coupon Statistics (Hiệu quả sử dụng mã giảm giá)
        var couponStats = completedOrders
            .Where(o => o.CouponId.HasValue && o.Coupon != null)
            .GroupBy(o => o.CouponId!.Value)
            .Select(g => new CouponUsageStatsDto
            {
                CouponId = g.Key,
                CouponCode = g.First().Coupon!.CouponCode,
                TimesUsed = g.Count(),
                TotalDiscountGiven = g.Sum(o => o.DiscountAmount ?? 0)
            })
            .OrderByDescending(c => c.TimesUsed)
            .ToList();

        return new DashboardDto
        {
            Overview = overview,
            DailyRevenue = dailyRevenue,
            PeakHours = peakHours,
            TopDrinks = popularDrinks,
            CouponStats = couponStats
        };
    }
}
