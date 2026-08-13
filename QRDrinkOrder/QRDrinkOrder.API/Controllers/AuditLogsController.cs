using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRDrinkOrder.API.Models;
using QRDrinkOrder.Shared.Constants;
using QRDrinkOrder.Shared.DTOs.Responses;

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
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? search = null)
        {
            var query = _context.AuditLogs
                .Include(l => l.Account)
                    .ThenInclude(a => a!.Role)
                .Include(l => l.Account)
                    .ThenInclude(a => a!.Employee)
                .Include(l => l.Account)
                    .ThenInclude(a => a!.Manager)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(l => l.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(l => l.CreatedAt <= endDate.Value);
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(l =>
                    (l.Account != null && l.Account.Email != null && l.Account.Email.Contains(search)) ||
                    (l.Account != null && l.Account.Employee != null && l.Account.Employee.FullName != null && l.Account.Employee.FullName.Contains(search)) ||
                    (l.Account != null && l.Account.Manager != null && l.Account.Manager.FullName != null && l.Account.Manager.FullName.Contains(search)) ||
                    (l.Action != null && l.Action.Contains(search)) ||
                    (l.TableName != null && l.TableName.Contains(search)) ||
                    (l.Description != null && l.Description.Contains(search)));

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
                    FullName = l.Account != null ? ((l.Account.RoleId == AppRoles.BaristaId || l.Account.RoleId == AppRoles.WaiterId) ? (l.Account.Employee != null ? l.Account.Employee.FullName : "Nhân viên") : (l.Account.Manager != null ? l.Account.Manager.FullName : "Quản lý")) : null,
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
