using QRDrinkOrder.API.Models;
using QRDrinkOrder.Shared.DTOs.Requests;

namespace QRDrinkOrder.API.Services.Interfaces;

public interface INotificationService
{
    Task SubscribeAsync(PushSubscriptionDto subscription);
    Task UnsubscribeAsync(string endpoint);
    Task SendNotificationToPhoneAsync(string phone, string title, string message, string url);
}
