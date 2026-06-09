namespace QRDrinkOrder.Shared.Models;

public partial class PointHistory
{
    public int HistoryId { get; set; }
    
    public string Phone { get; set; } = null!;
    
    public int PointsChanged { get; set; }
    
    public string Reason { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual Membership Membership { get; set; } = null!;
}
