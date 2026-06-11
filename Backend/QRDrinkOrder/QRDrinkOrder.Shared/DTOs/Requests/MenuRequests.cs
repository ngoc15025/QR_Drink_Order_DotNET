using System.ComponentModel.DataAnnotations;
using QRDrinkOrder.Shared.Enums;

namespace QRDrinkOrder.Shared.DTOs.Requests;

public class SaveCategoryRequest
{
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    [Required(ErrorMessage = "Tên danh mục tiếng Việt là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự.")]
    public string NameVi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên danh mục tiếng Anh là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự.")]
    public string NameEn { get; set; } = string.Empty;
}

public class SaveDrinkRequest
{
    [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Đường dẫn ảnh sản phẩm là bắt buộc.")]
    public string ImageUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Giá bán là bắt buộc.")]
    [Range(0, 1000000000, ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 0.")]
    public decimal BasePrice { get; set; }

    public DrinkTemperature TemperatureType { get; set; } = DrinkTemperature.Iced; // Default: 1 (Lạnh)

    public bool IsActive { get; set; } = true;

    [Required(ErrorMessage = "Tên món tiếng Việt là bắt buộc.")]
    [StringLength(150, ErrorMessage = "Tên món không được vượt quá 150 ký tự.")]
    public string NameVi { get; set; } = string.Empty;

    public string? DescriptionVi { get; set; }

    [Required(ErrorMessage = "Tên món tiếng Anh là bắt buộc.")]
    [StringLength(150, ErrorMessage = "Tên món không được vượt quá 150 ký tự.")]
    public string NameEn { get; set; } = string.Empty;

    public string? DescriptionEn { get; set; }
}

public class SavePromotionRequest
{
    [Required(ErrorMessage = "Đường dẫn ảnh Banner là bắt buộc.")]
    public string ImageUrl { get; set; } = string.Empty;

    public int? CouponId { get; set; }

    public bool IsActive { get; set; } = true;

    [Required(ErrorMessage = "Tiêu đề tiếng Việt là bắt buộc.")]
    [StringLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự.")]
    public string TitleVi { get; set; } = string.Empty;

    public string? ContentVi { get; set; }

    [Required(ErrorMessage = "Tiêu đề tiếng Anh là bắt buộc.")]
    [StringLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự.")]
    public string TitleEn { get; set; } = string.Empty;

    public string? ContentEn { get; set; }
}
