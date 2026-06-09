namespace QRDrinkOrder.Shared.Models;

public partial class AuditLog
{
    public int LogId { get; set; }

    public int? AccountId { get; set; }

    public string Action { get; set; } = null!;

    public string? TableName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Account? Account { get; set; }
}
