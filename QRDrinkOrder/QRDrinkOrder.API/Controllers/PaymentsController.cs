using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRDrinkOrder.API.Services.Interfaces;

namespace QRDrinkOrder.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, IConfiguration configuration, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _configuration = configuration;
        _logger = logger;
    }

    // SePay Webhook Payload
    public class SePayWebhookPayload
    {
        public int Id { get; set; }
        public string Gateway { get; set; } = string.Empty;
        public string TransactionDate { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string TransferType { get; set; } = string.Empty;
        public decimal TransferAmount { get; set; }
        public string Content { get; set; } = string.Empty; // Chứa mã QRORDER{Id}
        public string ReferenceCode { get; set; } = string.Empty; // Mã giao dịch ngân hàng
    }

    [HttpPost("sepay-webhook")]
    [AllowAnonymous] // Cho phép SePay gọi ẩn danh từ bên ngoài
    public async Task<IActionResult> SePayWebhook([FromBody] SePayWebhookPayload payload)
    {
        try
        {
            // Xác thực Webhook bằng Token
            var expectedToken = _configuration["SePay:WebhookToken"];
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();

            // Nếu chưa cấu hình token trên server thì cho phép pass luôn (dành cho môi trường test/demo)
            if (string.IsNullOrEmpty(expectedToken) || expectedToken.StartsWith("YOUR_"))
            {
                _logger.LogWarning("[SePay Webhook] Webhook token is not configured. Bypassing auth check for demo/testing.");
            }
            else if (string.IsNullOrEmpty(authHeader) || !authHeader.Contains(expectedToken))
            {
                _logger.LogWarning($"[SePay Webhook Warning] Unauthorized request. Header received: {authHeader}");
                return Unauthorized(new { Status = "unauthorized", Message = "Invalid or missing webhook token." });
            }

            // Ghi log để chẩn đoán
            _logger.LogInformation($"[SePay Webhook Received] ID: {payload.Id}, Content: '{payload.Content}', Amount: {payload.TransferAmount}, Ref: {payload.ReferenceCode}");

            if (payload.TransferType != null && payload.TransferType.ToLower() == "out")
            {
                // Bỏ qua giao dịch chuyển tiền đi
                return Ok(new { Status = "ignored", Message = "Transfer out type is ignored." });
            }

            var success = await _paymentService.ProcessSePayWebhookAsync(payload.Content, payload.TransferAmount, payload.ReferenceCode, payload.AccountNumber);
            if (success)
            {
                return Ok(new { Status = "success", Message = "Payment processed successfully." });
            }

            return BadRequest(new { Status = "failed", Message = "Payment process failed or order not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SePay Webhook Error] Process failed");
            return StatusCode(500, new { Status = "error", Message = "Đã xảy ra lỗi hệ thống khi xử lý thanh toán." });
        }
    }

    [HttpPost("{orderId}/confirm-cash")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public async Task<IActionResult> ConfirmCashPayment(int orderId)
    {
        try
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int employeeId))
            {
                return Unauthorized(new { Message = "Không tìm thấy thông tin nhân viên trong phiên làm việc." });
            }

            var success = await _paymentService.ConfirmCashPaymentAsync(orderId, employeeId);
            if (success)
            {
                return Ok(new { Message = "Xác nhận thanh toán tiền mặt thành công." });
            }

            return BadRequest(new { Message = "Không thể xác nhận thanh toán cho đơn hàng này." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming cash payment for Order {OrderId}", orderId);
            return BadRequest(new { Message = "Đã xảy ra lỗi hệ thống khi xác nhận thanh toán." });
        }
    }
}
