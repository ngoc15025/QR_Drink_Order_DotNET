using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRDrinkOrder.Shared.DTOs.Requests;
using QRDrinkOrder.Shared.DTOs.Responses;
using QRDrinkOrder.API.Models;

namespace QRDrinkOrder.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class BankAccountsController : ControllerBase
{
    private readonly QrdrinkOrderDbContext _context;

    public BankAccountsController(QrdrinkOrderDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous] // Allow anyone to get the active bank account if needed, but for now we'll allow all roles.
    public async Task<IActionResult> GetBankAccounts()
    {
        var accounts = await _context.BankAccounts
            .AsNoTracking()
            .Select(b => new BankAccountDto
            {
                BankAccountId = b.BankAccountId,
                BankCode = b.BankCode,
                AccountNumber = b.AccountNumber,
                AccountName = b.AccountName,
                IsActive = b.IsActive
            })
            .ToListAsync();
            
        return Ok(accounts);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBankAccount([FromBody] SaveBankAccountRequest request)
    {
        var bankAccount = new BankAccount
        {
            BankCode = request.BankCode,
            AccountNumber = request.AccountNumber,
            AccountName = request.AccountName,
            IsActive = request.IsActive
        };

        if (bankAccount.IsActive)
        {
            var activeAccounts = await _context.BankAccounts.Where(b => b.IsActive).ToListAsync();
            foreach (var acc in activeAccounts) acc.IsActive = false;
        }

        _context.BankAccounts.Add(bankAccount);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Thêm tài khoản ngân hàng thành công" });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBankAccount(int id, [FromBody] SaveBankAccountRequest request)
    {
        var bankAccount = await _context.BankAccounts.FindAsync(id);
        if (bankAccount == null) return NotFound();

        bankAccount.BankCode = request.BankCode;
        bankAccount.AccountNumber = request.AccountNumber;
        bankAccount.AccountName = request.AccountName;
        
        if (request.IsActive && !bankAccount.IsActive)
        {
            var activeAccounts = await _context.BankAccounts.Where(b => b.IsActive && b.BankAccountId != id).ToListAsync();
            foreach (var acc in activeAccounts) acc.IsActive = false;
            bankAccount.IsActive = true;
        }
        else if (!request.IsActive && bankAccount.IsActive)
        {
             bankAccount.IsActive = false;
        }

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Cập nhật tài khoản ngân hàng thành công" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBankAccount(int id)
    {
        var bankAccount = await _context.BankAccounts.FindAsync(id);
        if (bankAccount == null) return NotFound();

        _context.BankAccounts.Remove(bankAccount);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Xóa tài khoản ngân hàng thành công" });
    }

    [HttpPut("{id}/activate")]
    public async Task<IActionResult> ActivateBankAccount(int id)
    {
        var bankAccount = await _context.BankAccounts.FindAsync(id);
        if (bankAccount == null) return NotFound();

        var activeAccounts = await _context.BankAccounts.Where(b => b.IsActive).ToListAsync();
        foreach (var acc in activeAccounts) acc.IsActive = false;

        bankAccount.IsActive = true;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Đã đặt làm tài khoản mặc định" });
    }
}
