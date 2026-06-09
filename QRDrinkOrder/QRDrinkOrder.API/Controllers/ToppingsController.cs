using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRDrinkOrder.Shared.Models;

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
        var toppings = await _context.Toppings.AsNoTracking().ToListAsync();
        return Ok(toppings);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AddTopping([FromBody] Topping topping)
    {
        _context.Toppings.Add(topping);
        await _context.SaveChangesAsync();
        return Ok(topping);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateTopping(int id, [FromBody] Topping topping)
    {
        if (id != topping.ToppingId) return BadRequest();
        _context.Entry(topping).State = EntityState.Modified;
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
