using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRDrinkOrder.API.Services.Interfaces;
using QRDrinkOrder.Shared.Constants;
using QRDrinkOrder.Shared.DTOs.Requests;

namespace QRDrinkOrder.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAuthService _authService;

    public AccountsController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            if (response == null)
                return Unauthorized(new { Message = "Email hoặc mật khẩu không chính xác." });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var success = await _authService.RegisterAsync(request);
            if (success)
                return Ok(new { Message = "Đăng ký tài khoản thành công." });

            return BadRequest(new { Message = "Không thể tạo tài khoản." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var accountIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdClaim) || !int.TryParse(accountIdClaim, out int accountId))
                return Unauthorized(new { Message = ErrorMessages.Unauthorized });

            var success = await _authService.ChangePasswordAsync(accountId, request);
            if (success)
                return Ok(new { Message = "Thay đổi mật khẩu thành công." });

            return BadRequest(new { Message = "Thay đổi mật khẩu thất bại." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAllAccounts()
    {
        try
        {
            var list = await _authService.GetAllAccountsAsync();
            return Ok(list);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetAccountById(int id)
    {
        try
        {
            var account = await _authService.GetAccountByIdAsync(id);
            if (account == null)
                return NotFound(new { Message = "Không tìm thấy tài khoản." });

            return Ok(account);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> ToggleAccountStatus(int id, [FromBody] AccountStatusRequest request)
    {
        try
        {
            var success = await _authService.ToggleAccountStatusAsync(id, request.IsActive);
            if (success)
                return Ok(new { Message = "Cập nhật trạng thái tài khoản thành công." });

            return NotFound(new { Message = "Không tìm thấy tài khoản." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}
