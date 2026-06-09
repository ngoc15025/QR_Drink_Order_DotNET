namespace QRDrinkOrder.Shared.DTOs.Responses;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public byte RoleId { get; set; }
    public int AccountId { get; set; }
    public int? UserId { get; set; } // EmployeeID or ManagerID
}

public class AccountDto
{
    public int AccountId { get; set; }
    public string Email { get; set; } = string.Empty;
    public byte RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; }
}
