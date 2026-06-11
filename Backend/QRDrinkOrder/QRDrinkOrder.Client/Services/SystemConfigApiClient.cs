using QRDrinkOrder.Shared.DTOs;
using System.Net.Http.Json;

namespace QRDrinkOrder.Client.Services;

public class SystemConfigApiClient
{
    private readonly HttpClient _httpClient;

    public SystemConfigApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<SystemConfigDto>> GetConfigsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<SystemConfigDto>>("api/systemconfigs") ?? new List<SystemConfigDto>();
    }

    public async Task<bool> UpdateConfigAsync(string key, SystemConfigDto config)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/systemconfigs/{key}", config);
        return response.IsSuccessStatusCode;
    }
}
