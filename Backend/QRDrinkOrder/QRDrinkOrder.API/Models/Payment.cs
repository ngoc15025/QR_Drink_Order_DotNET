namespace QRDrinkOrder.API.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    public byte PaymentMethod { get; set; }

    public byte? PaymentStatus { get; set; }

    public string? TransactionId { get; set; }

    public decimal Amount { get; set; }

    public DateTime? PaidAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
