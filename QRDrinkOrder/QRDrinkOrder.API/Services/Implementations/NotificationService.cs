using Microsoft.EntityFrameworkCore;
using QRDrinkOrder.API.Services.Interfaces;
using QRDrinkOrder.Shared.Models;
using System.Text.Json;
using WebPush;

namespace QRDrinkOrder.API.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly QrdrinkOrderDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly WebPushClient _webPushClient;

    public NotificationService(QrdrinkOrderDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _webPushClient = new WebPushClient();
    }

    public async Task SubscribeAsync(QRDrinkOrder.Shared.Models.PushSubscription subscription)
    {
        var existing = await _context.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == subscription.Endpoint);
        if (existing == null)
        {
            _context.PushSubscriptions.Add(subscription);
        }
        else
        {
            existing.Phone = subscription.Phone;
            existing.P256DH = subscription.P256DH;
            existing.Auth = subscription.Auth;
        }
        await _context.SaveChangesAsync();
    }

    public async Task UnsubscribeAsync(string endpoint)
    {
        var existing = await _context.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == endpoint);
        if (existing != null)
        {
            _context.PushSubscriptions.Remove(existing);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SendNotificationToPhoneAsync(string phone, string title, string message, string url)
    {
        var subscriptions = await _context.PushSubscriptions
            .Where(s => s.Phone == phone)
            .ToListAsync();

        if (!subscriptions.Any()) return;

        var subject = _configuration["VapidSettings:Subject"];
        var publicKey = _configuration["VapidSettings:PublicKey"];
        var privateKey = _configuration["VapidSettings:PrivateKey"];
        var vapidDetails = new VapidDetails(subject, publicKey, privateKey);

        var payload = JsonSerializer.Serialize(new
        {
            title = title,
            message = message,
            url = url
        });

        foreach (var sub in subscriptions)
        {
            try
            {
                var pushSubscription = new WebPush.PushSubscription(sub.Endpoint, sub.P256DH, sub.Auth);
                await _webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
            }
            catch (WebPushException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.Gone || ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Subscription has expired or is no longer valid, remove it
                    _context.PushSubscriptions.Remove(sub);
                }
            }
            catch
            {
                // Ignore other errors
            }
        }
        
        await _context.SaveChangesAsync();
    }
}
