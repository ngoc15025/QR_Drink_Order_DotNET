using System.ComponentModel.DataAnnotations;
using QRDrinkOrder.Shared.Constants;
using QRDrinkOrder.Shared.Enums;

namespace QRDrinkOrder.Shared.DTOs.Responses;

public class CategoryDto
{
    public int CategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public string CategoryName { get; set; } = string.Empty; // Tên dựa theo ngôn ngữ hiện tại
    public List<CategoryTranslationDto> Translations { get; set; } = new();
}

public class CategoryTranslationDto
{
    public int CategoryTranslationId { get; set; }
    public int CategoryId { get; set; }
    public string LanguageCode { get; set; } = AppLanguages.Default;
    public string CategoryName { get; set; } = string.Empty;
}

public class DrinkDto
{
    public int DrinkId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public DrinkTemperature TemperatureType { get; set; }
    public bool IsActive { get; set; }
    public string DrinkName { get; set; } = string.Empty; // Tên món nước dựa theo ngôn ngữ hiện tại
    public string? Description { get; set; } // Mô tả dựa theo ngôn ngữ hiện tại
    public List<DrinkTranslationDto> Translations { get; set; } = new();
}

public class DrinkTranslationDto
{
    public int TranslationId { get; set; }
    public int DrinkId { get; set; }
    public string LanguageCode { get; set; } = AppLanguages.Default;
    public string DrinkName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class WeatherRecommendationDto
{
    public string WeatherCondition { get; set; } = string.Empty; // Nắng, Mưa, Nóng...
    public string RecommendationMessage { get; set; } = string.Empty;
    public List<DrinkDto> RecommendedDrinks { get; set; } = new();
}

public class PromotionDto
{
    public int PromotionId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int? CouponId { get; set; }
    public string? CouponCode { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? DiscountValue { get; set; }
    public byte? DiscountType { get; set; }
    public bool IsActive { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class SizeDto
{
    public int SizeId { get; set; }

    [Required(ErrorMessage = "Tên Size là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Tên Size không được vượt quá 50 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [Range(0, 1000000000, ErrorMessage = "Giá cộng thêm không được là số âm.")]
    public decimal PriceOffset { get; set; }
}

public class ToppingDto
{
    public int ToppingId { get; set; }

    [Required(ErrorMessage = "Tên Topping là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Tên Topping không được vượt quá 100 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [Range(0, 1000000000, ErrorMessage = "Giá Topping không được là số âm.")]
    public decimal Price { get; set; }
}
