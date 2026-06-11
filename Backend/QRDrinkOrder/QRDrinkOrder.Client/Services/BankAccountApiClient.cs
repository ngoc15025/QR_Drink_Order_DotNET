using QRDrinkOrder.Shared.DTOs.Requests;
using QRDrinkOrder.Shared.DTOs.Responses;
using System.Net.Http.Json;

namespace QRDrinkOrder.Client.Services;

public class BankAccountApiClient
{
    private readonly HttpClient _httpClient;

    public BankAccountApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<BankAccountDto>> GetBankAccountsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<BankAccountDto>>("api/bankaccounts") ?? new List<BankAccountDto>();
    }

    public async Task<bool> CreateBankAccountAsync(SaveBankAccountRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/bankaccounts", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateBankAccountAsync(int id, SaveBankAccountRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/bankaccounts/{id}", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteBankAccountAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/bankaccounts/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ActivateBankAccountAsync(int id)
    {
        var response = await _httpClient.PutAsync($"api/bankaccounts/{id}/activate", null);
        return response.IsSuccessStatusCode;
    }
}
