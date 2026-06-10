using System.Net.Http.Json;
using QRDrinkOrder.Client.Services.Interfaces;
using QRDrinkOrder.Shared.Models;

namespace QRDrinkOrder.Client.Services.Implementations;

public class AiRecommendationApiClient : IAiRecommendationApiClient
{
    private readonly HttpClient _httpClient;

    public AiRecommendationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AiRecommendationResult?> GetSmartSuggestionsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<AiRecommendationResult>("api/Recommendations/smart-suggest");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching AI recommendations: {ex.Message}");
            return null;
        }
    }
}
