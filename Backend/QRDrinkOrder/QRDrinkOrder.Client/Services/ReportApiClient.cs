using QRDrinkOrder.Shared.DTOs.Responses;
using System.Net.Http.Json;

namespace QRDrinkOrder.Client.Services;

public class ReportApiClient
{
    private readonly HttpClient _httpClient;

    public ReportApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DashboardDto?> GetDashboardDataAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var url = "api/reports/dashboard";
        var queryParams = new System.Collections.Generic.List<string>();

        if (startDate.HasValue)
            queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");

        if (endDate.HasValue)
            queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

        if (queryParams.Count > 0)
            url += "?" + string.Join("&", queryParams);

        try
        {
            return await _httpClient.GetFromJsonAsync<DashboardDto>(url);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
