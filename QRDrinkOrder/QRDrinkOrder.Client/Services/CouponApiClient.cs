using QRDrinkOrder.Shared.DTOs.Requests;
using QRDrinkOrder.Shared.DTOs.Responses;
using System.Net.Http.Json;

namespace QRDrinkOrder.Client.Services;

public class CouponApiClient
{
    private readonly HttpClient _httpClient;

    public CouponApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<CouponDto>> GetAllCouponsAsync(bool includeInactive = false)
    {
        return await _httpClient.GetFromJsonAsync<List<CouponDto>>($"api/coupons?includeInactive={includeInactive.ToString().ToLower()}") ?? new List<CouponDto>();
    }

    public async Task<bool> CreateCouponAsync(SaveCouponRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/coupons", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateCouponAsync(int id, SaveCouponRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/coupons/{id}", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteCouponAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/coupons/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<CouponDto?> ApplyCouponAsync(ApplyCouponRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/coupons/apply", request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<CouponDto>();
        }
        return null;
    }
}
