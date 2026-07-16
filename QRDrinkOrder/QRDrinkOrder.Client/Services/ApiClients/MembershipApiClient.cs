using QRDrinkOrder.Shared.DTOs.Responses;
using System.Net.Http.Json;

namespace QRDrinkOrder.Client.Services.ApiClients;

public class MembershipApiClient
{
    private readonly HttpClient _httpClient;

    public MembershipApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<MembershipDto?> GetMembershipByPhoneAsync(string phone)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<MembershipDto>($"api/memberships/{phone}");
        }
        catch
        {
            return null;
        }
    }
}
