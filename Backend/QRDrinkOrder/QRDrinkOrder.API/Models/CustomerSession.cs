namespace QRDrinkOrder.API.Models;

public partial class CustomerSession
{
    public Guid SessionId { get; set; }

    public string? Phone { get; set; }

    public string? PreferredLanguage { get; set; }

    public string? DeviceInfo { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ExpiredAt { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
