using Microsoft.AspNetCore.Mvc;
using QRDrinkOrder.API.Services.Interfaces;
using QRDrinkOrder.API.Models;

namespace QRDrinkOrder.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RecommendationsController : ControllerBase
{
    private readonly IAiRecommendationService _aiRecommendationService;

    public RecommendationsController(IAiRecommendationService aiRecommendationService)
    {
        _aiRecommendationService = aiRecommendationService;
    }

    [HttpGet("smart-suggest")]
    public async Task<ActionResult<AiRecommendationResult>> GetSmartSuggestions()
    {
        var result = await _aiRecommendationService.GetDrinkRecommendationsAsync();
        return Ok(result);
    }
}
