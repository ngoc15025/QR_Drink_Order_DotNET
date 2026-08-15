using QRDrinkOrder.Shared.Attributes;
using System.ComponentModel.DataAnnotations;

namespace QRDrinkOrder.Shared.DTOs.Requests;

public class CheckCustomerStatusRequest
{
    [RequiredString]
    public string Phone { get; set; } = string.Empty;
}

public class VerifyPinRequest
{
    [RequiredString]
    public string Phone { get; set; } = string.Empty;

    [RequiredString]
    [StringLength(6, MinimumLength = 4)]
    public string PinCode { get; set; } = string.Empty;
}

public class SetupPinRequest
{
    [RequiredString]
    public string Phone { get; set; } = string.Empty;

    [RequiredString]
    [StringLength(6, MinimumLength = 4)]
    public string PinCode { get; set; } = string.Empty;
}

public class ResetPinWithFirebaseRequest
{
    [RequiredString]
    public string Phone { get; set; } = string.Empty;

    [RequiredString]
    public string FirebaseIdToken { get; set; } = string.Empty;

    [RequiredString]
    [StringLength(6, MinimumLength = 4)]
    public string NewPinCode { get; set; } = string.Empty;
}

