using QRDrinkOrder.Shared.DTOs.Requests;
using QRDrinkOrder.Shared.DTOs.Responses;
using System.Net.Http.Json;

namespace QRDrinkOrder.Client.Services;

public class AuthApiClient
{
    private readonly HttpClient _httpClient;

    public AuthApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/accounts/login", request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AuthResponse>();
        }
        return null;
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/accounts/register", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<AccountDto>> GetAllAccountsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<AccountDto>>("api/accounts") ?? new List<AccountDto>();
    }

    public async Task<bool> ToggleAccountStatusAsync(int id, bool isActive)
    {
        var request = new AccountStatusRequest { IsActive = isActive };
        var response = await _httpClient.PutAsJsonAsync($"api/accounts/{id}/status", request);
        return response.IsSuccessStatusCode;
    }
}
