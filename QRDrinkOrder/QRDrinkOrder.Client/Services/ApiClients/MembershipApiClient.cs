using QRDrinkOrder.Shared.DTOs.Requests;
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

    public async Task<CheckCustomerStatusResponse?> CheckStatusAsync(string phone)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/memberships/check-status", new CheckCustomerStatusRequest { Phone = phone });
            return await response.Content.ReadFromJsonAsync<CheckCustomerStatusResponse>();
        }
        catch
        {
            return new CheckCustomerStatusResponse { Exists = false, IsPinSet = false, IsLocked = false, Message = "Lỗi kết nối tới máy chủ." };
        }
    }

    public async Task<CustomerAuthResponse?> VerifyPinAsync(VerifyPinRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/memberships/verify-pin", request);
            return await response.Content.ReadFromJsonAsync<CustomerAuthResponse>();
        }
        catch (Exception ex)
        {
            return new CustomerAuthResponse { Success = false, Message = ex.Message };
        }
    }

    public async Task<CustomerAuthResponse?> SetupPinAsync(SetupPinRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/memberships/setup-pin", request);
            return await response.Content.ReadFromJsonAsync<CustomerAuthResponse>();
        }
        catch (Exception ex)
        {
            return new CustomerAuthResponse { Success = false, Message = ex.Message };
        }
    }

    public async Task<CustomerAuthResponse?> ResetPinWithFirebaseAsync(ResetPinWithFirebaseRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/memberships/reset-pin-firebase", request);
            return await response.Content.ReadFromJsonAsync<CustomerAuthResponse>();
        }
        catch (Exception ex)
        {
            return new CustomerAuthResponse { Success = false, Message = ex.Message };
        }
    }
}
