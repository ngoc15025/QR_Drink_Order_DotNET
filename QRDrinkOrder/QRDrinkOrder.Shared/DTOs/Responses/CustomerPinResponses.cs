namespace QRDrinkOrder.Shared.DTOs.Responses;

public class CheckCustomerStatusResponse
{
    public bool Exists { get; set; }
    public bool IsPinSet { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public int FailedAttempts { get; set; }
    public string? Message { get; set; }
}

public class CustomerAuthResponse
{
    public bool Success { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public int Points { get; set; }
    public string? Message { get; set; }
}
