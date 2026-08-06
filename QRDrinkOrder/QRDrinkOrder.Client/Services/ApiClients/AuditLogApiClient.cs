using QRDrinkOrder.Shared.DTOs.Responses;
using System.Net.Http.Json;

namespace QRDrinkOrder.Client.Services.ApiClients
{
    public class AuditLogApiClient
    {
        private readonly HttpClient _httpClient;

        public AuditLogApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PagedAuditLogResponse?> GetAuditLogsAsync(int page = 1, int pageSize = 20, DateTime? startDate = null, DateTime? endDate = null, string? search = null)
        {
            var url = $"api/auditlogs?page={page}&pageSize={pageSize}";
            if (startDate.HasValue)
                url += $"&startDate={startDate.Value:yyyy-MM-dd}";
            if (endDate.HasValue)
                url += $"&endDate={endDate.Value:yyyy-MM-ddT23:59:59}";
            if (!string.IsNullOrWhiteSpace(search))
                url += $"&search={Uri.EscapeDataString(search)}";
            return await _httpClient.GetFromJsonAsync<PagedAuditLogResponse>(url);
        }
    }
}
