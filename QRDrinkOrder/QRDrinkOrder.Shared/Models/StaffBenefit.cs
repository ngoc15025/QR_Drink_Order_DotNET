namespace QRDrinkOrder.Shared.Models;

public partial class StaffBenefit
{
    public int BenefitId { get; set; }

    public int EmployeeId { get; set; }

    public int OrderId { get; set; }

    public DateOnly? UsedDate { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
