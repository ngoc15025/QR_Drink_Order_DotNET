using QRDrinkOrder.Shared.Models;
using System.Net.Http.Json;

namespace QRDrinkOrder.Client.Services;

public class SystemConfigApiClient
{
    private readonly HttpClient _httpClient;

    public SystemConfigApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<SystemConfig>> GetConfigsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<SystemConfig>>("api/systemconfigs") ?? new List<SystemConfig>();
    }

    public async Task<bool> UpdateConfigAsync(string key, SystemConfig config)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/systemconfigs/{key}", config);
        return response.IsSuccessStatusCode;
    }
}
