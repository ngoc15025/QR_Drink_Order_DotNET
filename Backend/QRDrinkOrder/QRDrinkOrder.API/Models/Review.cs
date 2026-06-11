namespace QRDrinkOrder.API.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public int OrderId { get; set; }

    public Guid SessionId { get; set; }

    public byte Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ICollection<ReviewImage> ReviewImages { get; set; } = new List<ReviewImage>();

    public virtual CustomerSession Session { get; set; } = null!;
}
