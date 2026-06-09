namespace QRDrinkOrder.Shared.Models;

public partial class PushSubscription
{
    public int SubscriptionId { get; set; }
    
    public string Phone { get; set; } = null!;
    
    public string Endpoint { get; set; } = null!;
    
    public string P256DH { get; set; } = null!;
    
    public string Auth { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
