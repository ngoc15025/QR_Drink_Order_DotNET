using QRDrinkOrder.Shared.DTOs;

namespace QRDrinkOrder.Client.Services.Interfaces;

public interface IAiRecommendationApiClient
{
    Task<AiRecommendationResultDto?> GetLatestRecommendationAsync();
}
