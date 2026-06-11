using System.Net.Http.Json;
using QRDrinkOrder.Client.Services.Interfaces;
using QRDrinkOrder.Shared.DTOs;

namespace QRDrinkOrder.Client.Services.Implementations;

public class AiRecommendationApiClient : IAiRecommendationApiClient
{
    private readonly HttpClient _httpClient;

    public AiRecommendationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AiRecommendationResultDto?> GetLatestRecommendationAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<AiRecommendationResultDto>("api/Recommendations/smart-suggest");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching AI recommendations: {ex.Message}");
            return null;
        }
    }
}
