using QRDrinkOrder.Shared.DTOs;

namespace QRDrinkOrder.Client.Services.ApiClients;

public interface IAiRecommendationApiClient
{
    Task<AiRecommendationResultDto?> GetLatestRecommendationAsync();
}
