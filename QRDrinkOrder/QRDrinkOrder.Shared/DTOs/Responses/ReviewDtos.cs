namespace QRDrinkOrder.Shared.DTOs.Responses;

public class ReviewDto
{
    public int ReviewId { get; set; }
    public int OrderId { get; set; }
    public Guid SessionId { get; set; }
    public byte Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public string? CustomerPhone { get; set; }
    public string? TableNumber { get; set; }
}
