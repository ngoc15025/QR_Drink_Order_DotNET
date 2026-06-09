using QRDrinkOrder.Shared.DTOs.Requests;
using QRDrinkOrder.Shared.DTOs.Responses;
using QRDrinkOrder.Shared.Models;
using System.Net.Http.Json;

namespace QRDrinkOrder.Client.Services;

public class MenuApiClient
{
    private readonly HttpClient _httpClient;

    public MenuApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync(string? lang = null, bool includeInactive = false)
    {
        lang ??= System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return await _httpClient.GetFromJsonAsync<List<CategoryDto>>($"api/menu/categories?lang={lang}&includeInactive={includeInactive.ToString().ToLower()}") ?? new List<CategoryDto>();
    }

    public async Task<List<DrinkDto>> GetDrinksAsync(string? lang = null, int? categoryId = null, bool includeInactive = false)
    {
        lang ??= System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var url = $"api/menu/drinks?lang={lang}&includeInactive={includeInactive.ToString().ToLower()}";
        if (categoryId.HasValue)
        {
            url += $"&categoryId={categoryId.Value}";
        }
        return await _httpClient.GetFromJsonAsync<List<DrinkDto>>(url) ?? new List<DrinkDto>();
    }

    public async Task<WeatherRecommendationDto?> GetWeatherRecommendationsAsync(string? lang = null, string weather = "nắng nóng")
    {
        lang ??= System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return await _httpClient.GetFromJsonAsync<WeatherRecommendationDto>($"api/menu/weather-recommendations?lang={lang}&weather={weather}");
    }

    public async Task<List<PromotionDto>> GetPromotionsAsync(string? lang = null, bool includeInactive = false)
    {
        lang ??= System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return await _httpClient.GetFromJsonAsync<List<PromotionDto>>($"api/menu/promotions?lang={lang}&includeInactive={includeInactive.ToString().ToLower()}") ?? new List<PromotionDto>();
    }

    // --- Admin/Manager Methods ---
    public async Task<bool> CreateCategoryAsync(SaveCategoryRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/menu/categories", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateCategoryAsync(int id, SaveCategoryRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/menu/categories/{id}", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/menu/categories/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateDrinkAsync(SaveDrinkRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/menu/drinks", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateDrinkAsync(int id, SaveDrinkRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/menu/drinks/{id}", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteDrinkAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/menu/drinks/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreatePromotionAsync(SavePromotionRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/menu/promotions", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdatePromotionAsync(int id, SavePromotionRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/menu/promotions/{id}", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeletePromotionAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/menu/promotions/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<string?> UploadImageAsync(MultipartFormDataContent content)
    {
        var response = await _httpClient.PostAsync("api/upload/image", content);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<UploadResponse>();
            return result?.Url;
        }
        return null;
    }

    // --- Size Methods ---
    public async Task<List<Size>> GetSizesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Size>>("api/sizes") ?? new List<Size>();
    }

    public async Task<bool> CreateSizeAsync(Size size)
    {
        var response = await _httpClient.PostAsJsonAsync("api/sizes", size);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateSizeAsync(int id, Size size)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/sizes/{id}", size);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteSizeAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/sizes/{id}");
        return response.IsSuccessStatusCode;
    }

    // --- Topping Methods ---
    public async Task<List<Topping>> GetToppingsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Topping>>("api/toppings") ?? new List<Topping>();
    }

    public async Task<bool> CreateToppingAsync(Topping topping)
    {
        var response = await _httpClient.PostAsJsonAsync("api/toppings", topping);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateToppingAsync(int id, Topping topping)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/toppings/{id}", topping);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteToppingAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/toppings/{id}");
        return response.IsSuccessStatusCode;
    }

    private class UploadResponse
    {
        public string Url { get; set; } = string.Empty;
    }
}
