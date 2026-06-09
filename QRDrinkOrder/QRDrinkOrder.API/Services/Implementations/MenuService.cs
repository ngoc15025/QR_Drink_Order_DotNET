using Microsoft.EntityFrameworkCore;
using QRDrinkOrder.API.Services.Interfaces;
using QRDrinkOrder.Shared.DTOs.Requests;
using QRDrinkOrder.Shared.DTOs.Responses;
using QRDrinkOrder.Shared.Models;

namespace QRDrinkOrder.API.Services.Implementations;

public class MenuService : IMenuService
{
    private readonly QrdrinkOrderDbContext _context;

    public MenuService(QrdrinkOrderDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync(string langCode, bool includeInactive = false)
    {
        var query = _context.Categories
            .AsNoTracking()
            .Include(c => c.CategoryTranslations)
            .AsSplitQuery()
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive == true);
        }

        var categories = await query.OrderBy(c => c.DisplayOrder).ToListAsync();

        return categories.Select(c => MapToCategoryDto(c, langCode)).ToList();
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id, string langCode)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .Include(c => c.CategoryTranslations)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category == null)
            return null;

        return MapToCategoryDto(category, langCode);
    }

    public async Task<CategoryDto> CreateCategoryAsync(SaveCategoryRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var category = new Category
            {
                DisplayOrder = request.DisplayOrder,
                IsActive = request.IsActive,
                CreatedAt = DateTime.Now
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var viTranslation = new CategoryTranslation
            {
                CategoryId = category.CategoryId,
                LanguageCode = "vi",
                CategoryName = request.NameVi
            };

            var enTranslation = new CategoryTranslation
            {
                CategoryId = category.CategoryId,
                LanguageCode = "en",
                CategoryName = request.NameEn
            };

            _context.CategoryTranslations.AddRange(viTranslation, enTranslation);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            // Load translations explicitly to return
            category.CategoryTranslations = new List<CategoryTranslation> { viTranslation, enTranslation };
            return MapToCategoryDto(category, "vi");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(int id, SaveCategoryRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var category = await _context.Categories
                .Include(c => c.CategoryTranslations)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return null;

            category.DisplayOrder = request.DisplayOrder;
            category.IsActive = request.IsActive;

            // Update translations
            var viTrans = category.CategoryTranslations.FirstOrDefault(t => t.LanguageCode.Trim() == "vi");
            if (viTrans != null)
                viTrans.CategoryName = request.NameVi;
            else
                _context.CategoryTranslations.Add(new CategoryTranslation { CategoryId = id, LanguageCode = "vi", CategoryName = request.NameVi });

            var enTrans = category.CategoryTranslations.FirstOrDefault(t => t.LanguageCode.Trim() == "en");
            if (enTrans != null)
                enTrans.CategoryName = request.NameEn;
            else
                _context.CategoryTranslations.Add(new CategoryTranslation { CategoryId = id, LanguageCode = "en", CategoryName = request.NameEn });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapToCategoryDto(category, "vi");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return false;

        // Xóa các món liên kết hoặc chuyển danh mục khác, ở đây ta ẩn hoặc báo lỗi nếu có nước
        var hasDrinks = await _context.Drinks.AnyAsync(d => d.CategoryId == id);
        if (hasDrinks)
            throw new Exception("Không thể xóa danh mục đang có sản phẩm.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<DrinkDto>> GetDrinksAsync(string langCode, int? categoryId = null, bool includeInactive = false)
    {
        var query = _context.Drinks
            .AsNoTracking()
            .Include(d => d.DrinkTranslations)
            .Include(d => d.Category)
                .ThenInclude(c => c.CategoryTranslations)
            .AsSplitQuery()
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(d => d.IsActive == true && d.Category.IsActive == true);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(d => d.CategoryId == categoryId.Value);
        }

        var drinks = await query.OrderByDescending(d => d.DrinkId).ToListAsync();

        return drinks.Select(d => MapToDrinkDto(d, langCode)).ToList();
    }

    public async Task<DrinkDto?> GetDrinkByIdAsync(int id, string langCode)
    {
        var drink = await _context.Drinks
            .AsNoTracking()
            .Include(d => d.DrinkTranslations)
            .Include(d => d.Category)
                .ThenInclude(c => c.CategoryTranslations)
            .AsSplitQuery()
            .FirstOrDefaultAsync(d => d.DrinkId == id);

        if (drink == null)
            return null;

        return MapToDrinkDto(drink, langCode);
    }

    public async Task<DrinkDto> CreateDrinkAsync(SaveDrinkRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var drink = new Drink
            {
                CategoryId = request.CategoryId,
                ImageUrl = request.ImageUrl,
                BasePrice = request.BasePrice,
                TemperatureType = request.TemperatureType,
                IsActive = request.IsActive,
                CreatedAt = DateTime.Now
            };

            _context.Drinks.Add(drink);
            await _context.SaveChangesAsync();

            var viTranslation = new DrinkTranslation
            {
                DrinkId = drink.DrinkId,
                LanguageCode = "vi",
                DrinkName = request.NameVi,
                Description = request.DescriptionVi
            };

            var enTranslation = new DrinkTranslation
            {
                DrinkId = drink.DrinkId,
                LanguageCode = "en",
                DrinkName = request.NameEn,
                Description = request.DescriptionEn
            };

            _context.DrinkTranslations.AddRange(viTranslation, enTranslation);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var loadedDrink = await _context.Drinks
                .Include(d => d.DrinkTranslations)
                .Include(d => d.Category)
                    .ThenInclude(c => c.CategoryTranslations)
                .FirstAsync(d => d.DrinkId == drink.DrinkId);

            return MapToDrinkDto(loadedDrink, "vi");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<DrinkDto?> UpdateDrinkAsync(int id, SaveDrinkRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var drink = await _context.Drinks
                .Include(d => d.DrinkTranslations)
                .FirstOrDefaultAsync(d => d.DrinkId == id);

            if (drink == null)
                return null;

            drink.CategoryId = request.CategoryId;
            drink.ImageUrl = request.ImageUrl;
            drink.BasePrice = request.BasePrice;
            drink.TemperatureType = request.TemperatureType;
            drink.IsActive = request.IsActive;

            // Update translations
            var viTrans = drink.DrinkTranslations.FirstOrDefault(t => t.LanguageCode.Trim() == "vi");
            if (viTrans != null)
            {
                viTrans.DrinkName = request.NameVi;
                viTrans.Description = request.DescriptionVi;
            }
            else
            {
                _context.DrinkTranslations.Add(new DrinkTranslation { DrinkId = id, LanguageCode = "vi", DrinkName = request.NameVi, Description = request.DescriptionVi });
            }

            var enTrans = drink.DrinkTranslations.FirstOrDefault(t => t.LanguageCode.Trim() == "en");
            if (enTrans != null)
            {
                enTrans.DrinkName = request.NameEn;
                enTrans.Description = request.DescriptionEn;
            }
            else
            {
                _context.DrinkTranslations.Add(new DrinkTranslation { DrinkId = id, LanguageCode = "en", DrinkName = request.NameEn, Description = request.DescriptionEn });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var loadedDrink = await _context.Drinks
                .Include(d => d.DrinkTranslations)
                .Include(d => d.Category)
                    .ThenInclude(c => c.CategoryTranslations)
                .FirstAsync(d => d.DrinkId == id);

            return MapToDrinkDto(loadedDrink, "vi");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteDrinkAsync(int id)
    {
        var drink = await _context.Drinks.FindAsync(id);
        if (drink == null)
            return false;

        // Kiểm tra xem món đã nằm trong hóa đơn nào chưa, nếu có thì ẩn chứ không xóa cứng
        var hasOrders = await _context.OrderItems.AnyAsync(oi => oi.DrinkId == id);
        if (hasOrders)
        {
            drink.IsActive = false; // Soft hide
        }
        else
        {
            _context.Drinks.Remove(drink);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<WeatherRecommendationDto> GetWeatherRecommendationsAsync(string langCode, string weatherType)
    {
        string message = "";
        List<DrinkDto> recommendedDrinks = new();

        var allDrinks = await _context.Drinks
            .AsNoTracking()
            .Include(d => d.DrinkTranslations)
            .Include(d => d.Category)
                .ThenInclude(c => c.CategoryTranslations)
            .AsSplitQuery()
            .Where(d => d.IsActive == true && d.Category.IsActive == true)
            .ToListAsync();

        if (weatherType.ToLower().Contains("hot") || weatherType.ToLower().Contains("nắng") || weatherType.ToLower().Contains("nóng"))
        {
            // Trời nóng gợi ý trà trái cây thanh nhiệt giải độc
            message = langCode == "vi"
                ? "Hôm nay trời nắng nóng! Thử ngay các món Trà Trái Cây mát lạnh để đập tan cơn khát nhé!"
                : "It's hot outside! Refresh yourself with our ice-cold Fruit Teas!";

            // Lấy các món có chứa từ "Trà" hoặc trong danh mục trà trái cây
            recommendedDrinks = allDrinks
                .Where(d => d.DrinkTranslations.Any(t => t.DrinkName.ToLower().Contains("trà") || t.DrinkName.ToLower().Contains("tea") || t.DrinkName.ToLower().Contains("đá")))
                .Take(4)
                .Select(d => MapToDrinkDto(d, langCode))
                .ToList();
        }
        else if (weatherType.ToLower().Contains("rain") || weatherType.ToLower().Contains("mưa") || weatherType.ToLower().Contains("lạnh"))
        {
            // Trời mưa lạnh gợi ý cà phê nóng hoặc trà sữa ấm áp
            message = langCode == "vi"
                ? "Thời tiết se lạnh hay có mưa! Một ly Cà Phê nóng đậm đà hoặc Trà Sữa ấm áp sẽ giúp bạn tỉnh táo và thư thái."
                : "It's rainy/chilly! Warm up your day with our premium Hot Espresso or delicious Hot Milk Teas.";

            recommendedDrinks = allDrinks
                .Where(d => d.DrinkTranslations.Any(t => t.DrinkName.ToLower().Contains("cà phê") || t.DrinkName.ToLower().Contains("coffee") || t.DrinkName.ToLower().Contains("nóng") || t.DrinkName.ToLower().Contains("hot")))
                .Take(4)
                .Select(d => MapToDrinkDto(d, langCode))
                .ToList();
        }
        else
        {
            // Thời tiết đẹp, gợi ý món bán chạy nhất hoặc món đặc trưng
            message = langCode == "vi"
                ? "Thời tiết tuyệt vời! Thưởng thức ngay những món uống Signature được yêu thích nhất tại cửa hàng."
                : "Wonderful weather! Treat yourself to our best-selling signature drinks.";

            recommendedDrinks = allDrinks
                .Take(4)
                .Select(d => MapToDrinkDto(d, langCode))
                .ToList();
        }

        // Fallback nếu không lọc được món nào phù hợp
        if (recommendedDrinks.Count == 0 && allDrinks.Count > 0)
        {
            recommendedDrinks = allDrinks.Take(4).Select(d => MapToDrinkDto(d, langCode)).ToList();
        }

        return new WeatherRecommendationDto
        {
            WeatherCondition = weatherType,
            RecommendationMessage = message,
            RecommendedDrinks = recommendedDrinks
        };
    }

    public async Task<List<PromotionDto>> GetPromotionsAsync(string langCode, bool includeInactive = false)
    {
        var query = _context.Promotions
            .AsNoTracking()
            .Include(p => p.PromotionTranslations)
            .Include(p => p.Coupon)
            .AsSplitQuery()
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive == true);
        }

        var promotions = await query
            .OrderByDescending(p => p.PromotionId)
            .ToListAsync();

        return promotions.Select(p =>
        {
            var trans = p.PromotionTranslations.FirstOrDefault(t => t.LanguageCode.Trim() == langCode)
                        ?? p.PromotionTranslations.FirstOrDefault(t => t.LanguageCode.Trim() == "vi")
                        ?? p.PromotionTranslations.FirstOrDefault();

            return new PromotionDto
            {
                PromotionId = p.PromotionId,
                ImageUrl = p.ImageUrl,
                CouponId = p.CouponId,
                CouponCode = p.Coupon?.CouponCode,
                StartDate = p.Coupon?.StartDate,
                EndDate = p.Coupon?.EndDate,
                DiscountValue = p.Coupon?.DiscountValue,
                DiscountType = p.Coupon?.DiscountType,
                IsActive = p.IsActive == true,
                Title = trans?.Title ?? "",
                Description = trans?.Content
            };
        }).ToList();
    }

    public async Task<PromotionDto?> GetPromotionByIdAsync(int id, string langCode)
    {
        var promotion = await _context.Promotions
            .AsNoTracking()
            .Include(p => p.PromotionTranslations)
            .Include(p => p.Coupon)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.PromotionId == id);

        if (promotion == null) return null;

        var trans = promotion.PromotionTranslations.FirstOrDefault(t => t.LanguageCode.Trim() == langCode)
                    ?? promotion.PromotionTranslations.FirstOrDefault(t => t.LanguageCode.Trim() == "vi")
                    ?? promotion.PromotionTranslations.FirstOrDefault();

        return new PromotionDto
        {
            PromotionId = promotion.PromotionId,
            ImageUrl = promotion.ImageUrl,
            CouponId = promotion.CouponId,
            CouponCode = promotion.Coupon?.CouponCode,
            StartDate = promotion.Coupon?.StartDate,
            EndDate = promotion.Coupon?.EndDate,
            DiscountValue = promotion.Coupon?.DiscountValue,
            DiscountType = promotion.Coupon?.DiscountType,
            IsActive = promotion.IsActive == true,
            Title = trans?.Title ?? "",
            Description = trans?.Content
        };
    }

    public async Task<PromotionDto> CreatePromotionAsync(SavePromotionRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var promotion = new Promotion
            {
                ImageUrl = request.ImageUrl,
                CouponId = request.CouponId,
                IsActive = request.IsActive,
                CreatedAt = DateTime.Now
            };

            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();

            var viTranslation = new PromotionTranslation
            {
                PromotionId = promotion.PromotionId,
                LanguageCode = "vi",
                Title = request.TitleVi,
                Content = request.ContentVi
            };

            var enTranslation = new PromotionTranslation
            {
                PromotionId = promotion.PromotionId,
                LanguageCode = "en",
                Title = request.TitleEn,
                Content = request.ContentEn
            };

            _context.PromotionTranslations.AddRange(viTranslation, enTranslation);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return new PromotionDto
            {
                PromotionId = promotion.PromotionId,
                ImageUrl = promotion.ImageUrl,
                CouponId = promotion.CouponId,
                IsActive = promotion.IsActive == true,
                Title = request.TitleVi,
                Description = request.ContentVi
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<PromotionDto?> UpdatePromotionAsync(int id, SavePromotionRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var promotion = await _context.Promotions
                .Include(p => p.PromotionTranslations)
                .FirstOrDefaultAsync(p => p.PromotionId == id);

            if (promotion == null) return null;

            promotion.ImageUrl = request.ImageUrl;
            promotion.CouponId = request.CouponId;
            promotion.IsActive = request.IsActive;

            var viTrans = promotion.PromotionTranslations.FirstOrDefault(t => t.LanguageCode.Trim() == "vi");
            if (viTrans != null)
            {
                viTrans.Title = request.TitleVi;
                viTrans.Content = request.ContentVi;
            }
            else
            {
                _context.PromotionTranslations.Add(new PromotionTranslation { PromotionId = id, LanguageCode = "vi", Title = request.TitleVi, Content = request.ContentVi });
            }

            var enTrans = promotion.PromotionTranslations.FirstOrDefault(t => t.LanguageCode.Trim() == "en");
            if (enTrans != null)
            {
                enTrans.Title = request.TitleEn;
                enTrans.Content = request.ContentEn;
            }
            else
            {
                _context.PromotionTranslations.Add(new PromotionTranslation { PromotionId = id, LanguageCode = "en", Title = request.TitleEn, Content = request.ContentEn });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new PromotionDto
            {
                PromotionId = promotion.PromotionId,
                ImageUrl = promotion.ImageUrl,
                CouponId = promotion.CouponId,
                IsActive = promotion.IsActive == true,
                Title = request.TitleVi,
                Description = request.ContentVi
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeletePromotionAsync(int id)
    {
        var promotion = await _context.Promotions.FindAsync(id);
        if (promotion == null) return false;

        _context.Promotions.Remove(promotion);
        await _context.SaveChangesAsync();
        return true;
    }

    private static CategoryDto MapToCategoryDto(Category category, string langCode)
    {
        var trans = category.CategoryTranslations.FirstOrDefault(t => t.LanguageCode.Trim() == langCode)
                    ?? category.CategoryTranslations.FirstOrDefault(t => t.LanguageCode.Trim() == "vi")
                    ?? category.CategoryTranslations.FirstOrDefault();

        return new CategoryDto
        {
            CategoryId = category.CategoryId,
            DisplayOrder = category.DisplayOrder ?? 0,
            IsActive = category.IsActive == true,
            CategoryName = trans?.CategoryName ?? "Chưa đặt tên",
            Translations = category.CategoryTranslations.Select(t => new CategoryTranslationDto
            {
                CategoryTranslationId = t.CategoryTranslationId,
                CategoryId = t.CategoryId,
                LanguageCode = t.LanguageCode.Trim(),
                CategoryName = t.CategoryName
            }).ToList()
        };
    }

    private static DrinkDto MapToDrinkDto(Drink drink, string langCode)
    {
        var trans = drink.DrinkTranslations.FirstOrDefault(t => t.LanguageCode.Trim() == langCode)
                    ?? drink.DrinkTranslations.FirstOrDefault(t => t.LanguageCode.Trim() == "vi")
                    ?? drink.DrinkTranslations.FirstOrDefault();

        var catTrans = drink.Category?.CategoryTranslations.FirstOrDefault(t => t.LanguageCode.Trim() == langCode)
                       ?? drink.Category?.CategoryTranslations.FirstOrDefault(t => t.LanguageCode.Trim() == "vi")
                       ?? drink.Category?.CategoryTranslations.FirstOrDefault();

        return new DrinkDto
        {
            DrinkId = drink.DrinkId,
            CategoryId = drink.CategoryId,
            CategoryName = catTrans?.CategoryName ?? "N/A",
            ImageUrl = drink.ImageUrl,
            BasePrice = drink.BasePrice,
            TemperatureType = drink.TemperatureType ?? QRDrinkOrder.Shared.Enums.DrinkTemperature.Iced,
            IsActive = drink.IsActive == true,
            DrinkName = trans?.DrinkName ?? "Chưa đặt tên",
            Description = trans?.Description,
            Translations = drink.DrinkTranslations.Select(t => new DrinkTranslationDto
            {
                TranslationId = t.TranslationId,
                DrinkId = t.DrinkId,
                LanguageCode = t.LanguageCode.Trim(),
                DrinkName = t.DrinkName,
                Description = t.Description
            }).ToList()
        };
    }
}
