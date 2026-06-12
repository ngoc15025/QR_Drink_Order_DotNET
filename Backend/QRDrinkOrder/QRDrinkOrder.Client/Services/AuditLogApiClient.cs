using System.Net.Http.Json;
using QRDrinkOrder.Shared.DTOs.Responses;

namespace QRDrinkOrder.Client.Services
{
    public class AuditLogApiClient
    {
        private readonly HttpClient _httpClient;

        public AuditLogApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PagedAuditLogResponse?> GetAuditLogsAsync(int page = 1, int pageSize = 20)
        {
            return await _httpClient.GetFromJsonAsync<PagedAuditLogResponse>($"api/auditlogs?page={page}&pageSize={pageSize}");
        }
    }
}
