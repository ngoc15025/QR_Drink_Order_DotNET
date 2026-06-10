using QRDrinkOrder.Shared.Models;

namespace QRDrinkOrder.Client.Services.Interfaces;

public interface IAiRecommendationApiClient
{
    Task<AiRecommendationResult?> GetSmartSuggestionsAsync();
}
