using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRDrinkOrder.Shared.Models;

namespace QRDrinkOrder.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SizesController : ControllerBase
{
    private readonly QrdrinkOrderDbContext _context;

    public SizesController(QrdrinkOrderDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSizes()
    {
        var sizes = await _context.Sizes.AsNoTracking().ToListAsync();
        return Ok(sizes);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AddSize([FromBody] Size size)
    {
        _context.Sizes.Add(size);
        await _context.SaveChangesAsync();
        return Ok(size);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateSize(int id, [FromBody] Size size)
    {
        if (id != size.SizeId) return BadRequest();
        _context.Entry(size).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteSize(int id)
    {
        var size = await _context.Sizes.FindAsync(id);
        if (size == null) return NotFound();
        _context.Sizes.Remove(size);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
