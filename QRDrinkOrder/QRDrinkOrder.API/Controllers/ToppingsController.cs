using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRDrinkOrder.API.Models;
using QRDrinkOrder.Shared.DTOs.Responses;

namespace QRDrinkOrder.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ToppingsController : ControllerBase
{
    private readonly QrdrinkOrderDbContext _context;

    public ToppingsController(QrdrinkOrderDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetToppings()
    {
        var toppings = await _context.Toppings.AsNoTracking().Select(t => new ToppingDto
        {
            ToppingId = t.ToppingId,
            Name = t.Name,
            Price = t.Price
        }).ToListAsync();
        return Ok(toppings);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AddTopping([FromBody] ToppingDto toppingDto)
    {
        var topping = new Topping
        {
            Name = toppingDto.Name,
            Price = toppingDto.Price
        };
        _context.Toppings.Add(topping);
        await _context.SaveChangesAsync();
        toppingDto.ToppingId = topping.ToppingId;
        return Ok(toppingDto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateTopping(int id, [FromBody] ToppingDto toppingDto)
    {
        if (id != toppingDto.ToppingId) return BadRequest();
        var topping = await _context.Toppings.FindAsync(id);
        if (topping == null) return NotFound();
        
        topping.Name = toppingDto.Name;
        topping.Price = toppingDto.Price;
        
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteTopping(int id)
    {
        var topping = await _context.Toppings.FindAsync(id);
        if (topping == null) return NotFound();
        _context.Toppings.Remove(topping);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
