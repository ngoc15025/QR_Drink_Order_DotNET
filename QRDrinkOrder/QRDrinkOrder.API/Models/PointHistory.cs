using QRDrinkOrder.Shared.Helpers;

namespace QRDrinkOrder.API.Models;

public partial class PointHistory
{
    public int HistoryId { get; set; }

    public string Phone { get; set; } = null!;

    public int PointsChanged { get; set; }

    public string Reason { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = TimeHelper.GetVietnamTime();

    public virtual Membership Membership { get; set; } = null!;
}
