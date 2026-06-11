namespace QRDrinkOrder.API.Models;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public int? CouponId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Coupon? Coupon { get; set; }

    public virtual ICollection<PromotionTranslation> PromotionTranslations { get; set; } = new List<PromotionTranslation>();
}
