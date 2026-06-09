namespace QRDrinkOrder.Shared.Models;

public partial class Manager
{
    public int ManagerId { get; set; }

    public int AccountId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public virtual Account Account { get; set; } = null!;
}
