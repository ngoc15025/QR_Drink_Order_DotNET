using System;

namespace QRDrinkOrder.Shared.DTOs;

public class AiRecommendationResultDto
{
    public int AiRecommendationResultId { get; set; }
    public string RecommendationText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
