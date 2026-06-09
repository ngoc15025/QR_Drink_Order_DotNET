using Microsoft.EntityFrameworkCore;
using QRDrinkOrder.API.Services.Interfaces;
using QRDrinkOrder.Shared.DTOs.Requests;
using QRDrinkOrder.Shared.DTOs.Responses;
using QRDrinkOrder.Shared.Models;

namespace QRDrinkOrder.API.Services.Implementations;

public class CouponService : ICouponService
{
    private readonly QrdrinkOrderDbContext _context;

    public CouponService(QrdrinkOrderDbContext context)
    {
        _context = context;
    }

    public async Task<List<CouponDto>> GetAllCouponsAsync(bool includeInactive = false)
    {
        var query = _context.Coupons.AsNoTracking().AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive == true && c.EndDate >= DateTime.Now);
        }

        var coupons = await query.ToListAsync();
        return coupons.Select(MapToDto).ToList();
    }

    public async Task<CouponDto?> GetCouponByIdAsync(int id)
    {
        var coupon = await _context.Coupons.AsNoTracking().FirstOrDefaultAsync(c => c.CouponId == id);
        if (coupon == null) return null;
        return MapToDto(coupon);
    }

    public async Task<CouponDto> CreateCouponAsync(SaveCouponRequest request)
    {
        var coupon = new Coupon
        {
            CouponCode = request.CouponCode.ToUpper(),
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MinOrderValue = request.MinOrderValue,
            MaxDiscountAmount = request.MaxDiscountAmount,
            UsageLimit = request.UsageLimit,
            UsedCount = 0,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        return MapToDto(coupon);
    }

    public async Task<CouponDto?> UpdateCouponAsync(int id, SaveCouponRequest request)
    {
        var coupon = await _context.Coupons.FindAsync(id);
        if (coupon == null) return null;

        coupon.CouponCode = request.CouponCode.ToUpper();
        coupon.DiscountType = request.DiscountType;
        coupon.DiscountValue = request.DiscountValue;
        coupon.MinOrderValue = request.MinOrderValue;
        coupon.MaxDiscountAmount = request.MaxDiscountAmount;
        coupon.UsageLimit = request.UsageLimit;
        coupon.StartDate = request.StartDate;
        coupon.EndDate = request.EndDate;
        coupon.IsActive = request.IsActive;

        await _context.SaveChangesAsync();
        return MapToDto(coupon);
    }

    public async Task<bool> DeleteCouponAsync(int id)
    {
        var coupon = await _context.Coupons.FindAsync(id);
        if (coupon == null) return false;

        _context.Coupons.Remove(coupon);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CouponDto?> ApplyCouponAsync(ApplyCouponRequest request)
    {
        var coupon = await _context.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CouponCode == request.CouponCode.ToUpper() && c.IsActive == true);

        if (coupon == null)
            throw new Exception("Mã giảm giá không tồn tại hoặc đã bị khóa.");

        if (coupon.StartDate > DateTime.Now)
            throw new Exception("Mã giảm giá chưa đến thời gian áp dụng.");

        if (coupon.EndDate < DateTime.Now)
            throw new Exception("Mã giảm giá đã hết hạn.");

        if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
            throw new Exception("Mã giảm giá đã hết lượt sử dụng.");

        if (request.OrderAmount < coupon.MinOrderValue)
            throw new Exception($"Đơn hàng chưa đạt giá trị tối thiểu {coupon.MinOrderValue:N0}đ để áp dụng mã.");

        return MapToDto(coupon);
    }

    private CouponDto MapToDto(Coupon coupon)
    {
        return new CouponDto
        {
            CouponId = coupon.CouponId,
            CouponCode = coupon.CouponCode,
            DiscountType = coupon.DiscountType,
            DiscountTypeName = coupon.DiscountType == 0 ? "Giảm tiền" : "Giảm %",
            DiscountValue = coupon.DiscountValue,
            MinOrderValue = coupon.MinOrderValue ?? 0m,
            MaxDiscountAmount = coupon.MaxDiscountAmount,
            UsageLimit = coupon.UsageLimit,
            UsedCount = coupon.UsedCount ?? 0,
            StartDate = coupon.StartDate,
            EndDate = coupon.EndDate,
            IsActive = coupon.IsActive == true
        };
    }
}
