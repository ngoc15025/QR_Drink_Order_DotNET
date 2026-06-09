using Microsoft.JSInterop;
using System.Net.Http.Json;
using QRDrinkOrder.Shared.Models;

namespace QRDrinkOrder.Client.Services;

public class PushNotificationService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _httpClient;

    public PushNotificationService(IJSRuntime jsRuntime, HttpClient httpClient)
    {
        _jsRuntime = jsRuntime;
        _httpClient = httpClient;
    }

    public async Task SubscribeToNotificationsAsync(string phone)
    {
        try
        {
            var subscription = await _jsRuntime.InvokeAsync<PushSubscriptionJs>("blazorPushNotifications.requestSubscription");
            if (subscription != null)
            {
                var subModel = new PushSubscription
                {
                    Phone = phone,
                    Endpoint = subscription.Url,
                    P256DH = subscription.P256dh,
                    Auth = subscription.Auth
                };

                await _httpClient.PostAsJsonAsync("api/notifications/subscribe", subModel);
            }
        }
        catch
        {
            // Ignore errors (user denied permission or browser not supported)
        }
    }

    private class PushSubscriptionJs
    {
        public string Url { get; set; } = "";
        public string P256dh { get; set; } = "";
        public string Auth { get; set; } = "";
    }
}
