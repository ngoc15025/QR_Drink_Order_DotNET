using QRDrinkOrder.Shared.Helpers;

namespace QRDrinkOrder.API.Models;

public partial class Membership
{
    public int MembershipId { get; set; }

    public string Phone { get; set; } = null!;

    public int Points { get; set; }

    public DateTime CreatedAt { get; set; } = TimeHelper.GetVietnamTime();

    public virtual ICollection<PointHistory> PointHistories { get; set; } = new List<PointHistory>();
}
