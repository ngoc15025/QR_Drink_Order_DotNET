namespace QRDrinkOrder.Shared.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public int? DisplayOrder { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<CategoryTranslation> CategoryTranslations { get; set; } = new List<CategoryTranslation>();

    public virtual ICollection<Drink> Drinks { get; set; } = new List<Drink>();
}
