using QRDrinkOrder.Shared.Attributes;
using System.ComponentModel.DataAnnotations;

namespace QRDrinkOrder.Shared.DTOs.Requests;

public class SaveCouponRequest
{
    [RequiredString(ErrorMessage = "Mã giảm giá là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Mã giảm giá không vượt quá 50 ký tự.")]
    public string CouponCode { get; set; } = string.Empty;

    [RequiredString(ErrorMessage = "Kiểu giảm giá là bắt buộc.")]
    public byte DiscountType { get; set; } // 0: Fixed, 1: Percentage

    [RequiredString(ErrorMessage = "Giá trị giảm giá là bắt buộc.")]
    [Range(0.01, 100000000, ErrorMessage = "Giá trị giảm phải lớn hơn 0.")]
    public decimal DiscountValue { get; set; }

    [Range(0, 100000000, ErrorMessage = "Giá trị đơn tối thiểu phải lớn hơn hoặc bằng 0.")]
    public decimal MinOrderValue { get; set; } = 0;

    public decimal? MaxDiscountAmount { get; set; }

    [Range(1, 100000000, ErrorMessage = "Giới hạn sử dụng phải lớn hơn 0.")]
    public int? UsageLimit { get; set; }

    [RequiredString(ErrorMessage = "Ngày bắt đầu là bắt buộc.")]
    public DateTime StartDate { get; set; } = DateTime.Now;

    [RequiredString(ErrorMessage = "Ngày kết thúc là bắt buộc.")]
    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;
}

public class ApplyCouponRequest
{
    [RequiredString(ErrorMessage = "Mã giảm giá không được trống.")]
    public string CouponCode { get; set; } = string.Empty;

    [RequiredString]
    [Range(0, 100000000)]
    public decimal OrderAmount { get; set; }

    [RequiredString(ErrorMessage = "Số điện thoại là bắt buộc để áp dụng mã giảm giá.")]
    public string Phone { get; set; } = string.Empty;
}

