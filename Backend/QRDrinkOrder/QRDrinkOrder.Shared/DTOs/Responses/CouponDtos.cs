namespace QRDrinkOrder.Shared.DTOs.Responses;

public class CouponDto
{
    public int CouponId { get; set; }
    public string CouponCode { get; set; } = string.Empty;
    public byte DiscountType { get; set; } // 0: Fixed, 1: Percentage
    public string DiscountTypeName { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal MinOrderValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired => DateTime.Now > EndDate;
    public bool IsLimitReached => UsageLimit.HasValue && UsedCount >= UsageLimit.Value;
}

public class ApplyCouponResponse
{
    public bool IsValid { get; set; }
    public decimal DiscountAmount { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? CouponId { get; set; }
}
