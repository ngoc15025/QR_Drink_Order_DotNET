using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QRDrinkOrder.API.Services.Interfaces;
using QRDrinkOrder.Shared.DTOs.Requests;
using QRDrinkOrder.Shared.DTOs.Responses;

namespace QRDrinkOrder.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembershipsController : ControllerBase
{
    private readonly IMembershipService _membershipService;

    public MembershipsController(IMembershipService membershipService)
    {
        _membershipService = membershipService;
    }

    [HttpGet("{phone}")]
    public async Task<IActionResult> GetMembership(string phone)
    {
        try
        {
            if (string.IsNullOrEmpty(phone))
                return BadRequest(new { Message = "Số điện thoại là bắt buộc." });

            var membership = await _membershipService.GetMembershipByPhoneAsync(phone);
            var monthlyCups = await _membershipService.GetMonthlyCupCountAsync(phone);

            if (membership == null)
            {
                return Ok(new MembershipDto { Phone = phone, Points = 0, IsPinSet = false, MonthlyCupCount = monthlyCups });
            }

            var dto = new MembershipDto
            {
                MembershipId = membership.MembershipId,
                Phone = membership.Phone,
                Points = membership.Points,
                IsPinSet = !string.IsNullOrEmpty(membership.PinCodeHash),
                MonthlyCupCount = monthlyCups,
                PointHistories = membership.PointHistories.Select(h => new PointHistoryDto
                {
                    HistoryId = h.HistoryId,
                    PointsChanged = h.PointsChanged,
                    Reason = h.Reason,
                    CreatedAt = h.CreatedAt
                }).ToList()
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("check-status")]
    public async Task<IActionResult> CheckStatus([FromBody] CheckCustomerStatusRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Phone))
                return BadRequest(new { Message = "Số điện thoại không hợp lệ." });

            var response = await _membershipService.CheckStatusAsync(request.Phone);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("verify-pin")]
    [EnableRateLimiting("LoginLimiter")]
    public async Task<IActionResult> VerifyPin([FromBody] VerifyPinRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new CustomerAuthResponse { Success = false, Message = "Dữ liệu yêu cầu không hợp lệ." });

            var response = await _membershipService.VerifyPinAsync(request);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new CustomerAuthResponse { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("setup-pin")]
    [EnableRateLimiting("LoginLimiter")]
    public async Task<IActionResult> SetupPin([FromBody] SetupPinRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new CustomerAuthResponse { Success = false, Message = "Dữ liệu yêu cầu không hợp lệ." });

            var response = await _membershipService.SetupPinAsync(request);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new CustomerAuthResponse { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("reset-pin-firebase")]
    [EnableRateLimiting("LoginLimiter")]
    public async Task<IActionResult> ResetPinWithFirebase([FromBody] ResetPinWithFirebaseRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new CustomerAuthResponse { Success = false, Message = "Dữ liệu yêu cầu không hợp lệ." });

            var response = await _membershipService.ResetPinWithFirebaseAsync(request);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new CustomerAuthResponse { Success = false, Message = ex.Message });
        }
    }
}
