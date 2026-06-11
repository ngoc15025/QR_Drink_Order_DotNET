namespace QRDrinkOrder.Shared.DTOs.Responses;

public class DashboardOverviewDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int ActiveCustomersCount { get; set; }
    public int ActiveCouponsCount { get; set; }
    public decimal CashRevenue { get; set; }
    public decimal SePayRevenue { get; set; }
}

public class DailyRevenueDto
{
    public string DateStr { get; set; } = string.Empty; // Định dạng dd/MM
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class PeakHourDto
{
    public int Hour { get; set; } // Giờ (0 - 23)
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class PopularDrinkDto
{
    public int DrinkId { get; set; }
    public string DrinkName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal RevenueGenerated { get; set; }
}

public class CouponUsageStatsDto
{
    public int CouponId { get; set; }
    public string CouponCode { get; set; } = string.Empty;
    public int TimesUsed { get; set; }
    public decimal TotalDiscountGiven { get; set; }
}

public class DashboardDto
{
    public DashboardOverviewDto Overview { get; set; } = new();
    public List<DailyRevenueDto> DailyRevenue { get; set; } = new();
    public List<PeakHourDto> PeakHours { get; set; } = new();
    public List<PopularDrinkDto> TopDrinks { get; set; } = new();
    public List<CouponUsageStatsDto> CouponStats { get; set; } = new();
}
