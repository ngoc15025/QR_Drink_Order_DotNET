using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QRDrinkOrder.API.Models;
using System.Security.Claims;

namespace QRDrinkOrder.API.Interceptors
{
    public class AuditLogInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            AddAuditLogs(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            AddAuditLogs(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void AddAuditLogs(DbContext? context)
        {
            if (context == null) return;

            var httpContext = _httpContextAccessor.HttpContext;
            int? accountId = null;

            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                // In AuthService we store AccountId or UserId. Let's try to get AccountId.
                // It usually maps to ClaimTypes.NameIdentifier or a custom claim "AccountId"
                var idClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == "AccountId" || c.Type == ClaimTypes.NameIdentifier);
                if (idClaim != null && int.TryParse(idClaim.Value, out var id))
                {
                    accountId = id;
                }
            }

            context.ChangeTracker.DetectChanges();
            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .Where(e => !(e.Entity is AuditLog)) // Do not log the log itself
                .Where(e => !(e.Entity is CustomerSession)) // Optional: ignore frequent updates on sessions if any
                .ToList();

            foreach (var entry in entries)
            {
                var action = entry.State switch
                {
                    EntityState.Added => "INSERT",
                    EntityState.Modified => "UPDATE",
                    EntityState.Deleted => "DELETE",
                    _ => "UNKNOWN"
                };

                var tableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name;
                var description = "";

                if (entry.State == EntityState.Added)
                {
                    description = $"Thêm mới bản ghi vào bảng {tableName}";
                }
                else if (entry.State == EntityState.Deleted)
                {
                    var idProp = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                    var idVal = idProp != null ? idProp.OriginalValue : "N/A";
                    description = $"Xóa bản ghi (ID: {idVal}) khỏi bảng {tableName}";
                }
                else if (entry.State == EntityState.Modified)
                {
                    var idProp = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                    var idVal = idProp != null ? idProp.OriginalValue : "N/A";
                    
                    var changes = entry.Properties
                        .Where(p => p.IsModified)
                        .Select(p => $"{p.Metadata.Name}: '{p.OriginalValue}' -> '{p.CurrentValue}'");
                        
                    description = $"Cập nhật bản ghi (ID: {idVal}): " + string.Join(", ", changes);
                }

                var auditLog = new AuditLog
                {
                    AccountId = accountId,
                    Action = action,
                    TableName = tableName,
                    Description = description,
                    CreatedAt = DateTime.UtcNow
                };

                context.Set<AuditLog>().Add(auditLog);
            }
        }
    }
}
