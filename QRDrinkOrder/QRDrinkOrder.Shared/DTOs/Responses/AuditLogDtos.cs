using System;
using System.Collections.Generic;

namespace QRDrinkOrder.Shared.DTOs.Responses
{
    public class AuditLogDto
    {
        public int LogId { get; set; }
        public int? AccountId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string Action { get; set; } = null!;
        public string? TableName { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class PagedAuditLogResponse
    {
        public List<AuditLogDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
