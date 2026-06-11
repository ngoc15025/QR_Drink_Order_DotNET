using Microsoft.EntityFrameworkCore;
using QRDrinkOrder.API.Services.Interfaces;
using QRDrinkOrder.API.Models;

namespace QRDrinkOrder.API.Services.Implementations;

public class MembershipService : IMembershipService
{
    private readonly QrdrinkOrderDbContext _context;

    public MembershipService(QrdrinkOrderDbContext context)
    {
        _context = context;
    }

    public async Task<Membership?> GetMembershipByPhoneAsync(string phone)
    {
        var membership = await _context.Memberships
            .Include(m => m.PointHistories.OrderByDescending(h => h.CreatedAt).Take(10))
            .FirstOrDefaultAsync(m => m.Phone == phone);
            
        return membership;
    }
}
