using QRDrinkOrder.Shared.DTOs.Responses;

namespace QRDrinkOrder.API.Services.Interfaces;

public interface IReportService
{
    Task<DashboardDto> GetDashboardDataAsync(DateTime? startDate = null, DateTime? endDate = null);
}
