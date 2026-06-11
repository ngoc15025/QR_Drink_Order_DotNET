namespace QRDrinkOrder.API.Models;

public partial class PromotionTranslation
{
    public int TranslationId { get; set; }

    public int PromotionId { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public virtual Promotion Promotion { get; set; } = null!;
}
