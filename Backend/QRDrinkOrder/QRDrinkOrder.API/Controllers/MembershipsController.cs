using Microsoft.AspNetCore.Mvc;
using QRDrinkOrder.API.Services.Interfaces;
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
            
            if (membership == null)
            {
                return Ok(new MembershipDto { Phone = phone, Points = 0 });
            }

            var dto = new MembershipDto
            {
                MembershipId = membership.MembershipId,
                Phone = membership.Phone,
                Points = membership.Points,
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
}
