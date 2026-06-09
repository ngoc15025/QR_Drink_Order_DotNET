namespace QRDrinkOrder.Shared.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public int AccountId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<StaffBenefit> StaffBenefits { get; set; } = new List<StaffBenefit>();
}
