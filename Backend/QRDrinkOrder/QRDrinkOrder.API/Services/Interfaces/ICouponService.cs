using QRDrinkOrder.Shared.DTOs.Requests;
using QRDrinkOrder.Shared.DTOs.Responses;

namespace QRDrinkOrder.API.Services.Interfaces;

public interface ICouponService
{
    Task<List<CouponDto>> GetAllCouponsAsync(bool includeInactive = false);
    Task<CouponDto?> GetCouponByIdAsync(int id);
    Task<CouponDto> CreateCouponAsync(SaveCouponRequest request);
    Task<CouponDto?> UpdateCouponAsync(int id, SaveCouponRequest request);
    Task<bool> DeleteCouponAsync(int id);
    Task<CouponDto?> ApplyCouponAsync(ApplyCouponRequest request);
}
