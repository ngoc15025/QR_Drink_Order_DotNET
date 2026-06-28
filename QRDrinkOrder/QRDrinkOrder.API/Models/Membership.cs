using System.ComponentModel.DataAnnotations;

namespace QRDrinkOrder.API.Models;

public partial class Membership
{
    public int MembershipId { get; set; }
    
    public string Phone { get; set; } = null!;
    
    public int Points { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual ICollection<PointHistory> PointHistories { get; set; } = new List<PointHistory>();
}
