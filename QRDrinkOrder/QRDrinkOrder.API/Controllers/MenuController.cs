using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRDrinkOrder.API.Services.Interfaces;
using QRDrinkOrder.Shared.DTOs.Requests;

namespace QRDrinkOrder.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MenuController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    // ----------------------------------------------------
    // PHẦN DANH MỤC (CATEGORIES)
    // ----------------------------------------------------

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories([FromQuery] string lang = "vi", [FromQuery] bool includeInactive = false)
    {
        try
        {
            var list = await _menuService.GetCategoriesAsync(lang, includeInactive);
            return Ok(list);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("categories/{id}")]
    public async Task<IActionResult> GetCategoryById(int id, [FromQuery] string lang = "vi")
    {
        try
        {
            var category = await _menuService.GetCategoryByIdAsync(id, lang);
            if (category == null)
                return NotFound(new { Message = "Không tìm thấy danh mục." });

            return Ok(category);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("categories")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateCategory([FromBody] SaveCategoryRequest request)
    {
        try
        {
            var category = await _menuService.CreateCategoryAsync(request);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.CategoryId }, category);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("categories/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] SaveCategoryRequest request)
    {
        try
        {
            var category = await _menuService.UpdateCategoryAsync(id, request);
            if (category == null)
                return NotFound(new { Message = "Không tìm thấy danh mục." });

            return Ok(category);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("categories/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            var success = await _menuService.DeleteCategoryAsync(id);
            if (success)
                return Ok(new { Message = "Xóa danh mục thành công." });

            return NotFound(new { Message = "Không tìm thấy danh mục." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    // ----------------------------------------------------
    // PHẦN MÓN NƯỚC (DRINKS)
    // ----------------------------------------------------

    [HttpGet("drinks")]
    public async Task<IActionResult> GetDrinks([FromQuery] string lang = "vi", [FromQuery] int? categoryId = null, [FromQuery] bool includeInactive = false)
    {
        try
        {
            var list = await _menuService.GetDrinksAsync(lang, categoryId, includeInactive);
            return Ok(list);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("drinks/{id}")]
    public async Task<IActionResult> GetDrinkById(int id, [FromQuery] string lang = "vi")
    {
        try
        {
            var drink = await _menuService.GetDrinkByIdAsync(id, lang);
            if (drink == null)
                return NotFound(new { Message = "Không tìm thấy món nước." });

            return Ok(drink);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("drinks")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateDrink([FromBody] SaveDrinkRequest request)
    {
        try
        {
            var drink = await _menuService.CreateDrinkAsync(request);
            return CreatedAtAction(nameof(GetDrinkById), new { id = drink.DrinkId }, drink);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("drinks/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateDrink(int id, [FromBody] SaveDrinkRequest request)
    {
        try
        {
            var drink = await _menuService.UpdateDrinkAsync(id, request);
            if (drink == null)
                return NotFound(new { Message = "Không tìm thấy món nước." });

            return Ok(drink);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("drinks/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteDrink(int id)
    {
        try
        {
            var success = await _menuService.DeleteDrinkAsync(id);
            if (success)
                return Ok(new { Message = "Xóa món nước thành công." });

            return NotFound(new { Message = "Không tìm thấy món nước." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    // ----------------------------------------------------
    // GỢI Ý THỜI TIẾT
    // ----------------------------------------------------

    [HttpGet("weather-recommendations")]
    public async Task<IActionResult> GetWeatherRecommendations([FromQuery] string lang = "vi", [FromQuery] string weather = "nắng nóng")
    {
        try
        {
            var recommendations = await _menuService.GetWeatherRecommendationsAsync(lang, weather);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    // ----------------------------------------------------
    // KHUYẾN MÃI / TIN TỨC
    // ----------------------------------------------------
    [HttpGet("promotions")]
    public async Task<IActionResult> GetPromotions([FromQuery] string lang = "vi", [FromQuery] bool includeInactive = false)
    {
        try
        {
            var promotions = await _menuService.GetPromotionsAsync(lang, includeInactive);
            return Ok(promotions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("promotions/{id}")]
    public async Task<IActionResult> GetPromotionById(int id, [FromQuery] string lang = "vi")
    {
        var promotion = await _menuService.GetPromotionByIdAsync(id, lang);
        if (promotion == null) return NotFound(new { Message = "Không tìm thấy tin tức/khuyến mãi." });
        return Ok(promotion);
    }

    [HttpPost("promotions")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreatePromotion([FromBody] SavePromotionRequest request)
    {
        try
        {
            var promotion = await _menuService.CreatePromotionAsync(request);
            return CreatedAtAction(nameof(GetPromotionById), new { id = promotion.PromotionId }, promotion);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("promotions/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdatePromotion(int id, [FromBody] SavePromotionRequest request)
    {
        try
        {
            var promotion = await _menuService.UpdatePromotionAsync(id, request);
            if (promotion == null) return NotFound(new { Message = "Không tìm thấy tin tức/khuyến mãi." });
            return Ok(promotion);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("promotions/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeletePromotion(int id)
    {
        try
        {
            var success = await _menuService.DeletePromotionAsync(id);
            if (success) return Ok(new { Message = "Xóa tin tức/khuyến mãi thành công." });
            return NotFound(new { Message = "Không tìm thấy tin tức/khuyến mãi." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}
