using System.ComponentModel.DataAnnotations;

namespace QRDrinkOrder.Shared.DTOs.Requests;

public class CheckCustomerStatusRequest
{
    [Required]
    public string Phone { get; set; } = string.Empty;
}

public class VerifyPinRequest
{
    [Required]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [StringLength(6, MinimumLength = 4)]
    public string PinCode { get; set; } = string.Empty;
}

public class SetupPinRequest
{
    [Required]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [StringLength(6, MinimumLength = 4)]
    public string PinCode { get; set; } = string.Empty;
}

public class ResetPinWithFirebaseRequest
{
    [Required]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public string FirebaseIdToken { get; set; } = string.Empty;

    [Required]
    [StringLength(6, MinimumLength = 4)]
    public string NewPinCode { get; set; } = string.Empty;
}
