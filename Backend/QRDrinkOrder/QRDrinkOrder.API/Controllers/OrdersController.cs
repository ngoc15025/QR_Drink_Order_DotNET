using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRDrinkOrder.API.Services.Interfaces;
using QRDrinkOrder.Shared.Constants;
using QRDrinkOrder.Shared.DTOs.Requests;

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
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return BadRequest(new { Message = "Đã xảy ra lỗi hệ thống khi tạo đơn hàng." });
        }
    }

    [HttpGet("active")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public async Task<IActionResult> GetActiveOrders()
    {
        try
        {
            var list = await _orderService.GetActiveOrdersAsync();
            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active orders");
            return BadRequest(new { Message = "Đã xảy ra lỗi hệ thống khi lấy danh sách đơn hàng." });
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetOrderHistoryByPhone([FromQuery] string phone)
    {
        try
        {
            if (string.IsNullOrEmpty(phone))
                return BadRequest(new { Message = "Số điện thoại là bắt buộc để tra cứu." });

            var list = await _orderService.GetOrderHistoryByPhoneAsync(phone);
            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order history");
            return BadRequest(new { Message = "Đã xảy ra lỗi hệ thống khi tra cứu đơn hàng." });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound(new { Message = ErrorMessages.OrderNotFound });

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order by id");
            return BadRequest(new { Message = "Đã xảy ra lỗi hệ thống khi lấy thông tin đơn hàng." });
        }
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateStatus Error");
            return BadRequest(new { Message = "Đã xảy ra lỗi hệ thống khi cập nhật trạng thái." });
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(int id, [FromHeader(Name = "X-Session-ID")] string sessionIdHeader)
    {
        try
        {
            if (string.IsNullOrEmpty(sessionIdHeader) || !Guid.TryParse(sessionIdHeader, out Guid sessionId))
                return BadRequest(new { Message = "Session-ID không hợp lệ hoặc thiếu." });

            var success = await _orderService.CancelOrderAsync(id, sessionId);
            if (success)
                return Ok(new { Message = "Hủy đơn hàng thành công." });

            return NotFound(new { Message = ErrorMessages.OrderNotFound });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order");
            return BadRequest(new { Message = "Đã xảy ra lỗi hệ thống khi hủy đơn hàng." });
        }
    }

    [HttpPut("{id}/payment-method")]
    public async Task<IActionResult> UpdatePaymentMethod(int id, [FromBody] byte paymentMethod, [FromHeader(Name = "X-Session-ID")] string sessionIdHeader)
    {
        try
        {
            if (string.IsNullOrEmpty(sessionIdHeader) || !Guid.TryParse(sessionIdHeader, out Guid sessionId))
                return BadRequest(new { Message = "Session-ID không hợp lệ hoặc thiếu." });

            var success = await _orderService.UpdatePaymentMethodAsync(id, sessionId, paymentMethod);
            if (success)
                return Ok(new { Message = "Cập nhật phương thức thanh toán thành công." });

            return NotFound(new { Message = "Không thể thay đổi phương thức thanh toán." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment method");
            return BadRequest(new { Message = "Đã xảy ra lỗi hệ thống khi cập nhật phương thức thanh toán." });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAllOrders([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync(startDate, endDate);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all orders");
            return BadRequest(new { Message = "Đã xảy ra lỗi hệ thống khi lấy danh sách đơn hàng." });
        }
    }
}
