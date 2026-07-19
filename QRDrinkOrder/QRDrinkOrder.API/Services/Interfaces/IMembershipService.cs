using QRDrinkOrder.API.Models;
using QRDrinkOrder.Shared.DTOs.Requests;
using QRDrinkOrder.Shared.DTOs.Responses;

namespace QRDrinkOrder.API.Services.Interfaces;

public interface IMembershipService
{
    Task<Membership?> GetMembershipByPhoneAsync(string phone);
    Task<CheckCustomerStatusResponse> CheckStatusAsync(string phone);
    Task<CustomerAuthResponse> VerifyPinAsync(VerifyPinRequest request);
    Task<CustomerAuthResponse> SetupPinAsync(SetupPinRequest request);
    Task<CustomerAuthResponse> ResetPinWithFirebaseAsync(ResetPinWithFirebaseRequest request);
    Task<int> GetMonthlyCupCountAsync(string phone);
}
