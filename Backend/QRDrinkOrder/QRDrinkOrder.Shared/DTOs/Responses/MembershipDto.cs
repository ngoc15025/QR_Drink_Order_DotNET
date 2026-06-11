namespace QRDrinkOrder.Shared.DTOs.Responses;

public class MembershipDto
{
    public int MembershipId { get; set; }
    public string Phone { get; set; } = string.Empty;
    public int Points { get; set; }
    public List<PointHistoryDto> PointHistories { get; set; } = new();
}

public class PointHistoryDto
{
    public int HistoryId { get; set; }
    public int PointsChanged { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
