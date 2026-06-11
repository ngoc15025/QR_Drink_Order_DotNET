using System;

namespace QRDrinkOrder.Shared.DTOs.Requests;

public class PushSubscriptionDto
{
    public string Phone { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string P256DH { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
}
