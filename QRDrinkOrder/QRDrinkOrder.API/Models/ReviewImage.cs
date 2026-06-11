namespace QRDrinkOrder.API.Models;

public partial class ReviewImage
{
    public int ImageId { get; set; }

    public int ReviewId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public DateTime? UploadedAt { get; set; }

    public virtual Review Review { get; set; } = null!;
}
