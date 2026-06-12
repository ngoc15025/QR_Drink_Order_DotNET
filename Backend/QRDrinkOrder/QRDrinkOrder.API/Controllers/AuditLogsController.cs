using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRDrinkOrder.API.Models;
using QRDrinkOrder.Shared.DTOs.Responses;
using QRDrinkOrder.Shared.Constants;
using System.Linq;
using System.Threading.Tasks;

namespace QRDrinkOrder.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
    public class AuditLogsController : ControllerBase
    {
        private readonly QrdrinkOrderDbContext _context;

        public AuditLogsController(QrdrinkOrderDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = _context.AuditLogs
                .Include(l => l.Account)
                    .ThenInclude(a => a.Role)
                .Include(l => l.Account)
                    .ThenInclude(a => a.Employee)
                .Include(l => l.Account)
                    .ThenInclude(a => a.Manager)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new AuditLogDto
                {
                    LogId = l.LogId,
                    AccountId = l.AccountId,
                    Email = l.Account != null ? l.Account.Email : null,
                    FullName = l.Account != null ? (l.Account.RoleId == AppRoles.EmployeeId ? (l.Account.Employee != null ? l.Account.Employee.FullName : "Nhân viên") : (l.Account.Manager != null ? l.Account.Manager.FullName : "Quản lý")) : null,
                    Action = l.Action,
                    TableName = l.TableName,
                    Description = l.Description,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            var response = new PagedAuditLogResponse
            {
                Items = logs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Ok(response);
        }
    }
}
