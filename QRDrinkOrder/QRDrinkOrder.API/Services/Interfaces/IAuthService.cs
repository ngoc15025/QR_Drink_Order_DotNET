using QRDrinkOrder.Shared.DTOs.Requests;
using QRDrinkOrder.Shared.DTOs.Responses;

namespace QRDrinkOrder.API.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<bool> RegisterAsync(RegisterRequest request, byte callerRoleId);
    Task<bool> ChangePasswordAsync(int accountId, ChangePasswordRequest request);
    Task<bool> ToggleAccountStatusAsync(int accountId, bool isActive, byte callerRoleId);
    Task<bool> UpdateAccountAsync(int accountId, UpdateAccountRequest request, byte callerRoleId);
    Task<bool> ResetPasswordAsync(int accountId, string newPassword, byte callerRoleId);
    Task<List<AccountDto>> GetAllAccountsAsync();
    Task<AccountDto?> GetAccountByIdAsync(int accountId);
}
