using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRDrinkOrder.API.Models;
using QRDrinkOrder.Shared.DTOs;
using QRDrinkOrder.Shared.DTOs.Responses;

namespace QRDrinkOrder.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemConfigsController : ControllerBase
{
    private readonly QrdrinkOrderDbContext _context;

    public SystemConfigsController(QrdrinkOrderDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetConfigs()
    {
        var configs = await _context.SystemConfigs.AsNoTracking().Select(c => new SystemConfigDto
        {
            ConfigKey = c.ConfigKey,
            ConfigValue = c.ConfigValue,
            Description = c.Description
        }).ToListAsync();
        return Ok(configs);
    }

    [HttpPut("{key}")]
    public async Task<IActionResult> UpdateConfig(string key, [FromBody] SystemConfigDto update)
    {
        var config = await _context.SystemConfigs.FindAsync(key);
        if (config == null) return NotFound();

        config.ConfigValue = update.ConfigValue;
        // Optionally update description
        if (!string.IsNullOrEmpty(update.Description))
            config.Description = update.Description;

        await _context.SaveChangesAsync();
        return Ok(new SystemConfigDto
        {
            ConfigKey = config.ConfigKey,
            ConfigValue = config.ConfigValue,
            Description = config.Description
        });
    }
}
