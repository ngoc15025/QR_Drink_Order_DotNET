namespace QRDrinkOrder.API.Models;

public partial class Drink
{
    public int DrinkId { get; set; }

    public int CategoryId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public decimal BasePrice { get; set; }

    public QRDrinkOrder.Shared.Enums.DrinkTemperature? TemperatureType { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<DrinkTranslation> DrinkTranslations { get; set; } = new List<DrinkTranslation>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
