namespace QRDrinkOrder.Shared.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public Guid SessionId { get; set; }

    public int? EmployeeId { get; set; }

    public string? TableNumber { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? FinalAmount { get; set; }

    public byte? OrderStatus { get; set; }

    public int? CouponId { get; set; }

    public int? PointsUsed { get; set; }

    public DateTime? OrderDate { get; set; }

    public string? Note { get; set; }

    public virtual Coupon? Coupon { get; set; }

    public virtual ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Payment? Payment { get; set; }

    public virtual Review? Review { get; set; }

    public virtual CustomerSession Session { get; set; } = null!;

    public virtual ICollection<StaffBenefit> StaffBenefits { get; set; } = new List<StaffBenefit>();
}
