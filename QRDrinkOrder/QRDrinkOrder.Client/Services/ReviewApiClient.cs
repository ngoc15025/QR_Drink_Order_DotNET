using QRDrinkOrder.Shared.DTOs.Requests;
using QRDrinkOrder.Shared.DTOs.Responses;
using System.Net.Http.Json;

namespace QRDrinkOrder.Client.Services;

public class ReviewApiClient
{
    private readonly HttpClient _httpClient;

    public ReviewApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> SubmitReviewAsync(Guid sessionId, SubmitReviewRequest request)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/reviews");
        httpRequest.Headers.Add("X-Session-ID", sessionId.ToString());
        httpRequest.Content = JsonContent.Create(request);

        var response = await _httpClient.SendAsync(httpRequest);
        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        throw new Exception(errorContent);
    }

    public async Task<List<ReviewDto>> GetReviewsForDrinkAsync(int drinkId)
    {
        var reviews = await _httpClient.GetFromJsonAsync<List<ReviewDto>>($"api/reviews/drink/{drinkId}") ?? new List<ReviewDto>();
        foreach (var review in reviews)
        {
            if (review.ImageUrls != null)
            {
                review.ImageUrls = review.ImageUrls.Select(url => ImageUrlResolver.Resolve(url, _httpClient.BaseAddress?.ToString())).ToList();
            }
        }
        return reviews;
    }

    public async Task<List<ReviewDto>> GetAllReviewsAsync()
    {
        var reviews = await _httpClient.GetFromJsonAsync<List<ReviewDto>>("api/reviews") ?? new List<ReviewDto>();
        foreach (var review in reviews)
        {
            if (review.ImageUrls != null)
            {
                review.ImageUrls = review.ImageUrls.Select(url => ImageUrlResolver.Resolve(url, _httpClient.BaseAddress?.ToString())).ToList();
            }
        }
        return reviews;
    }
}
