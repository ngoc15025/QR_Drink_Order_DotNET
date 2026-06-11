namespace QRDrinkOrder.API.Models;

public partial class DrinkTranslation
{
    public int TranslationId { get; set; }

    public int DrinkId { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string DrinkName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual Drink Drink { get; set; } = null!;
}
