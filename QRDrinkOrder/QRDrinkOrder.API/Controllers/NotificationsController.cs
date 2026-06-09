using Microsoft.AspNetCore.Mvc;
using QRDrinkOrder.API.Services.Interfaces;
using QRDrinkOrder.Shared.Models;

namespace QRDrinkOrder.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] PushSubscription subscription)
    {
        if (string.IsNullOrEmpty(subscription.Phone) || string.IsNullOrEmpty(subscription.Endpoint))
            return BadRequest("Thông tin thuê bao không hợp lệ");

        await _notificationService.SubscribeAsync(subscription);
        return Ok();
    }

    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] string endpoint)
    {
        if (string.IsNullOrEmpty(endpoint))
            return BadRequest("Endpoint không hợp lệ");

        await _notificationService.UnsubscribeAsync(endpoint);
        return Ok();
    }
}
