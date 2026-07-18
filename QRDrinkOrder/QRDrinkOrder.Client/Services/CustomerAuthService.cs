using QRDrinkOrder.Client.Services;

namespace QRDrinkOrder.Client.Services;

public class CustomerAuthService
{
    private readonly LocalStorageService _localStorage;
    private const string PhoneKey = "CustomerPhone";
    private const string TokenKey = "CustomerAuthToken";

    public string? CurrentPhone { get; private set; }
    public string? CurrentToken { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentPhone) && !string.IsNullOrEmpty(CurrentToken);

    public event Action? OnAuthStateChanged;

    public CustomerAuthService(LocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task InitializeAsync()
    {
        CurrentPhone = await _localStorage.GetItemAsync(PhoneKey);
        CurrentToken = await _localStorage.GetItemAsync(TokenKey);
        OnAuthStateChanged?.Invoke();
    }

    public async Task<string?> GetAuthTokenAsync()
    {
        if (string.IsNullOrEmpty(CurrentToken))
        {
            await InitializeAsync();
        }
        return CurrentToken;
    }

    public async Task LoginAsync(string phone, string token)
    {
        CurrentPhone = phone;
        CurrentToken = token;
        await _localStorage.SetItemAsync(PhoneKey, phone);
        await _localStorage.SetItemAsync(TokenKey, token);
        OnAuthStateChanged?.Invoke();
    }

    public async Task LogoutAsync()
    {
        CurrentPhone = null;
        CurrentToken = null;
        await _localStorage.RemoveItemAsync(PhoneKey);
        await _localStorage.RemoveItemAsync(TokenKey);
        OnAuthStateChanged?.Invoke();
    }
}
