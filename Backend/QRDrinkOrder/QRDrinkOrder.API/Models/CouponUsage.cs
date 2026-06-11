namespace QRDrinkOrder.API.Models;

public partial class CouponUsage
{
    public int UsageId { get; set; }

    public int CouponId { get; set; }

    public string Phone { get; set; } = null!;

    public int OrderId { get; set; }

    public DateTime? UsedAt { get; set; }

    public virtual Coupon Coupon { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
