using System.ComponentModel.DataAnnotations;

namespace QRDrinkOrder.Shared.DTOs.Requests;

public class SaveBankAccountRequest
{
    [Required]
    [StringLength(50)]
    public string BankCode { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string AccountNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string AccountName { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
