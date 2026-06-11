using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRDrinkOrder.API.Services.Interfaces;
using QRDrinkOrder.Shared.DTOs.Requests;

namespace QRDrinkOrder.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouponsController : ControllerBase
{
    private readonly ICouponService _couponService;

    public CouponsController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAllCoupons([FromQuery] bool includeInactive = false)
    {
        try
        {
            var coupons = await _couponService.GetAllCouponsAsync(includeInactive);
            return Ok(coupons);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetCouponById(int id)
    {
        try
        {
            var coupon = await _couponService.GetCouponByIdAsync(id);
            if (coupon == null) return NotFound(new { Message = "Không tìm thấy mã giảm giá." });
            return Ok(coupon);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateCoupon([FromBody] SaveCouponRequest request)
    {
        try
        {
            var coupon = await _couponService.CreateCouponAsync(request);
            return CreatedAtAction(nameof(GetCouponById), new { id = coupon.CouponId }, coupon);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateCoupon(int id, [FromBody] SaveCouponRequest request)
    {
        try
        {
            var coupon = await _couponService.UpdateCouponAsync(id, request);
            if (coupon == null) return NotFound(new { Message = "Không tìm thấy mã giảm giá." });
            return Ok(coupon);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteCoupon(int id)
    {
        try
        {
            var success = await _couponService.DeleteCouponAsync(id);
            if (success) return Ok(new { Message = "Xóa mã giảm giá thành công." });
            return NotFound(new { Message = "Không tìm thấy mã giảm giá." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("apply")]
    public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponRequest request)
    {
        try
        {
            var coupon = await _couponService.ApplyCouponAsync(request);
            return Ok(coupon);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}
