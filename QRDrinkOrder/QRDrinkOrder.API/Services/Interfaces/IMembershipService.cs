using QRDrinkOrder.Shared.Models;

namespace QRDrinkOrder.API.Services.Interfaces;

public interface IMembershipService
{
    Task<Membership?> GetMembershipByPhoneAsync(string phone);
}
