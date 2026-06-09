using QRDrinkOrder.Shared.Models;

namespace QRDrinkOrder.API.Services.Interfaces;

public interface INotificationService
{
    Task SubscribeAsync(QRDrinkOrder.Shared.Models.PushSubscription subscription);
    Task UnsubscribeAsync(string endpoint);
    Task SendNotificationToPhoneAsync(string phone, string title, string message, string url);
}
