using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRDrinkOrder.API.Services.Interfaces;

namespace QRDrinkOrder.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardData([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var data = await _reportService.GetDashboardDataAsync(startDate, endDate);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}
