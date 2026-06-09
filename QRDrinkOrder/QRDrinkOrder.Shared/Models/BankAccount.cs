using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QRDrinkOrder.Shared.Models;

public partial class BankAccount
{
    [Key]
    public int BankAccountId { get; set; }

    [Required]
    [StringLength(50)]
    public string BankCode { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string AccountNumber { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string AccountName { get; set; } = null!;

    public bool IsActive { get; set; }
}
