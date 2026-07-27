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

    public (byte EffectiveType, decimal EffectiveValue) GetEffectiveDiscount()
    {
        // 1. Nếu ghi là Giảm % (DiscountType = 1) nhưng giá trị > 100 (Ví dụ: 20000), đây là Giảm tiền mặt 20.000đ
        if (DiscountType == 1 && DiscountValue > 100)
        {
            return (0, DiscountValue);
        }
        // 2. Nếu ghi là Giảm tiền mặt (DiscountType = 0) nhưng giá trị <= 100 (Ví dụ: 15), đây là Giảm 15%
        if (DiscountType == 0 && DiscountValue <= 100)
        {
            return (1, DiscountValue);
        }
        return (DiscountType, DiscountValue);
    }

    public decimal? GetEffectiveMaxDiscount()
    {
        if (!MaxDiscountAmount.HasValue || MaxDiscountAmount.Value <= 0) return null;
        // Nếu nhập trần quá nhỏ <= 100 (Ví dụ: nhập 15 thay vì 15000), quy đổi thành 15.000đ
        if (MaxDiscountAmount.Value <= 100) return MaxDiscountAmount.Value * 1000m;
        return MaxDiscountAmount.Value;
    }
}

public class ApplyCouponResponse
{
    public bool IsValid { get; set; }
    public decimal DiscountAmount { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? CouponId { get; set; }
}
