namespace QRDrinkOrder.API.Models;

public partial class CategoryTranslation
{
    public int CategoryTranslationId { get; set; }

    public int CategoryId { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string CategoryName { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;
}
