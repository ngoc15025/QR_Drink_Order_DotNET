namespace QRDrinkOrder.Shared.DTOs;

public class SystemConfigDto
{
    public string ConfigKey { get; set; } = null!;
    public string ConfigValue { get; set; } = null!;
    public string? Description { get; set; }
}
