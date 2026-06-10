using QRDrinkOrder.Shared.Models;

namespace QRDrinkOrder.API.Services.Interfaces;

public interface IAiRecommendationService
{
    Task<AiRecommendationResult> GetDrinkRecommendationsAsync();
}
