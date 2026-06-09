using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRDrinkOrder.API.Services.Interfaces;
using QRDrinkOrder.Shared.DTOs.Requests;

namespace QRDrinkOrder.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    public async Task<IActionResult> SubmitReview([FromBody] SubmitReviewRequest request, [FromHeader(Name = "X-Session-ID")] string? sessionIdHeader)
    {
        try
        {
            if (string.IsNullOrEmpty(sessionIdHeader) || !Guid.TryParse(sessionIdHeader, out Guid sessionId))
            {
                return BadRequest(new { Message = "Thiếu thông tin phiên khách hàng (X-Session-ID)." });
            }

            var success = await _reviewService.SubmitReviewAsync(sessionId, request);
            if (success)
            {
                return Ok(new { Message = "Gửi đánh giá thành công. Cảm ơn ý kiến của bạn!" });
            }

            return BadRequest(new { Message = "Không thể gửi đánh giá." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("drink/{drinkId}")]
    public async Task<IActionResult> GetReviewsForDrink(int drinkId)
    {
        try
        {
            var list = await _reviewService.GetReviewsForDrinkAsync(drinkId);
            return Ok(list);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAllReviews()
    {
        try
        {
            var list = await _reviewService.GetAllReviewsAsync();
            return Ok(list);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}
