namespace QRDrinkOrder.Shared.DTOs.Responses;

public class BankAccountDto
{
    public int BankAccountId { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
