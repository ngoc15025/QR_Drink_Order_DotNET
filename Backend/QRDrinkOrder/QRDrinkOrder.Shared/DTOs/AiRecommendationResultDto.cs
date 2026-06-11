using System;
using System.Collections.Generic;

namespace QRDrinkOrder.Shared.DTOs;

public class AiRecommendationResultDto
{
    public string Message { get; set; } = string.Empty;
    public List<int> DrinkIds { get; set; } = new List<int>();

    // For backwards compatibility or legacy uses
    public int AiRecommendationResultId { get; set; }
    public string RecommendationText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
