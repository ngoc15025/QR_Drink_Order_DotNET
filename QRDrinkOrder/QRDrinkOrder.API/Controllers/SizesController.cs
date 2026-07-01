using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRDrinkOrder.API.Models;
using QRDrinkOrder.Shared.DTOs.Responses;

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
        var sizes = await _context.Sizes.AsNoTracking().Select(s => new SizeDto
        {
            SizeId = s.SizeId,
            Name = s.Name,
            PriceOffset = s.PriceOffset
        }).ToListAsync();
        return Ok(sizes);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AddSize([FromBody] SizeDto sizeDto)
    {
        if (sizeDto.PriceOffset < 0) return BadRequest("Giá cộng thêm không được là số âm.");
        var size = new Size
        {
            Name = sizeDto.Name,
            PriceOffset = sizeDto.PriceOffset
        };
        _context.Sizes.Add(size);
        await _context.SaveChangesAsync();
        sizeDto.SizeId = size.SizeId;
        return Ok(sizeDto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateSize(int id, [FromBody] SizeDto sizeDto)
    {
        if (id != sizeDto.SizeId) return BadRequest();
        if (sizeDto.PriceOffset < 0) return BadRequest("Giá cộng thêm không được là số âm.");
        var size = await _context.Sizes.FindAsync(id);
        if (size == null) return NotFound();
        
        size.Name = sizeDto.Name;
        size.PriceOffset = sizeDto.PriceOffset;
        
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
