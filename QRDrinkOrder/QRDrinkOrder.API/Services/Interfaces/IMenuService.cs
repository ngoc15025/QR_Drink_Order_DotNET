using QRDrinkOrder.Shared.DTOs.Requests;
using QRDrinkOrder.Shared.DTOs.Responses;

namespace QRDrinkOrder.API.Services.Interfaces;

public interface IMenuService
{
    Task<List<CategoryDto>> GetCategoriesAsync(string langCode, bool includeInactive = false);
    Task<CategoryDto?> GetCategoryByIdAsync(int id, string langCode);
    Task<CategoryDto> CreateCategoryAsync(SaveCategoryRequest request);
    Task<CategoryDto?> UpdateCategoryAsync(int id, SaveCategoryRequest request);
    Task<bool> DeleteCategoryAsync(int id);

    Task<List<DrinkDto>> GetDrinksAsync(string langCode, int? categoryId = null, bool includeInactive = false);
    Task<DrinkDto?> GetDrinkByIdAsync(int id, string langCode);
    Task<DrinkDto> CreateDrinkAsync(SaveDrinkRequest request);
    Task<DrinkDto?> UpdateDrinkAsync(int id, SaveDrinkRequest request);
    Task<bool> DeleteDrinkAsync(int id);

    Task<WeatherRecommendationDto> GetWeatherRecommendationsAsync(string langCode, string weatherType);
    Task<List<PromotionDto>> GetPromotionsAsync(string langCode, bool includeInactive = false);
    Task<PromotionDto?> GetPromotionByIdAsync(int id, string langCode);
    Task<PromotionDto> CreatePromotionAsync(SavePromotionRequest request);
    Task<PromotionDto?> UpdatePromotionAsync(int id, SavePromotionRequest request);
    Task<bool> DeletePromotionAsync(int id);
}
