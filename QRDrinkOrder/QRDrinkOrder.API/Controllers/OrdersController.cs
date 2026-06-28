using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRDrinkOrder.API.Services.Interfaces;
using QRDrinkOrder.Shared.Constants;
using QRDrinkOrder.Shared.DTOs.Requests;
using Microsoft.AspNetCore.RateLimiting;

namespace QRDrinkOrder.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, [FromHeader(Name = "X-Session-ID")] string? sessionIdHeader)
    {
        if (string.IsNullOrEmpty(sessionIdHeader) || !Guid.TryParse(sessionIdHeader, out Guid sessionId))
        {
            // Nếu FE chưa gửi UUID thì sinh tự động
            sessionId = Guid.NewGuid();
        }

        // Nếu người gọi đã đăng nhập là nhân viên đặt món hộ
        if (User.Identity?.IsAuthenticated == true && request.EmployeeId == null)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int empId))
            {
                request.EmployeeId = empId;
            }
        }

        var order = await _orderService.CreateOrderAsync(sessionId, request);
        return Ok(order);
    }

    [HttpGet("active")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public async Task<IActionResult> GetActiveOrders()
    {
        var list = await _orderService.GetActiveOrdersAsync();
        return Ok(list);
    }

    [HttpGet("history")]
    [EnableRateLimiting("LoginLimiter")]
    public async Task<IActionResult> GetOrderHistoryByPhone([FromQuery] string phone)
    {
        if (string.IsNullOrEmpty(phone))
            return BadRequest(new { Message = "Số điện thoại là bắt buộc để tra cứu." });

        var list = await _orderService.GetOrderHistoryByPhoneAsync(phone);
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound(new { Message = ErrorMessages.OrderNotFound });

        return Ok(order);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        var roleIdClaim = User.FindFirst("RoleId")?.Value;
        var userIdClaim = User.FindFirst("UserId")?.Value;
        int? employeeId = null;
        // Chỉ ghi nhận EmployeeId nếu role là Employee (RoleId = "3")
        if (!string.IsNullOrEmpty(roleIdClaim) && roleIdClaim == "3" && !string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int empId))
        {
            employeeId = empId;
        }

        var success = await _orderService.UpdateOrderStatusAsync(id, request.OrderStatus, employeeId);
        if (success)
            return Ok(new { Message = "Cập nhật trạng thái đơn hàng thành công." });

        return NotFound(new { Message = ErrorMessages.OrderNotFound });
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(int id, [FromHeader(Name = "X-Session-ID")] string sessionIdHeader)
    {
        if (string.IsNullOrEmpty(sessionIdHeader) || !Guid.TryParse(sessionIdHeader, out Guid sessionId))
            return BadRequest(new { Message = "Session-ID không hợp lệ hoặc thiếu." });

        var success = await _orderService.CancelOrderAsync(id, sessionId);
        if (success)
            return Ok(new { Message = "Hủy đơn hàng thành công." });

        return NotFound(new { Message = ErrorMessages.OrderNotFound });
    }

    [HttpPut("{id}/payment-method")]
    public async Task<IActionResult> UpdatePaymentMethod(int id, [FromBody] byte paymentMethod, [FromHeader(Name = "X-Session-ID")] string sessionIdHeader)
    {
        if (string.IsNullOrEmpty(sessionIdHeader) || !Guid.TryParse(sessionIdHeader, out Guid sessionId))
            return BadRequest(new { Message = "Session-ID không hợp lệ hoặc thiếu." });

        var success = await _orderService.UpdatePaymentMethodAsync(id, sessionId, paymentMethod);
        if (success)
            return Ok(new { Message = "Cập nhật phương thức thanh toán thành công." });

        return NotFound(new { Message = "Không thể thay đổi phương thức thanh toán." });
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAllOrders([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var orders = await _orderService.GetAllOrdersAsync(startDate, endDate);
        return Ok(orders);
    }
}
