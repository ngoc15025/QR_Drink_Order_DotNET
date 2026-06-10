namespace QRDrinkOrder.Shared.Models;

public class AiRecommendationResult
{
    public string Message { get; set; } = string.Empty;
    public List<int> DrinkIds { get; set; } = new List<int>();
}
