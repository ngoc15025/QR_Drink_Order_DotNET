using QRDrinkOrder.Shared.DTOs.Requests;
using QRDrinkOrder.Shared.DTOs.Responses;

namespace QRDrinkOrder.API.Services.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(Guid sessionId, CreateOrderRequest request);
    Task<List<OrderDto>> GetActiveOrdersAsync();
    Task<List<OrderDto>> GetOrderHistoryByPhoneAsync(string phone);
    Task<OrderDto?> GetOrderByIdAsync(int orderId);
    Task<bool> UpdateOrderStatusAsync(int orderId, byte status, int? employeeId = null);
    Task<bool> CancelOrderAsync(int orderId, Guid sessionId);
    Task<bool> UpdatePaymentMethodAsync(int orderId, Guid sessionId, byte paymentMethod);
    Task<List<OrderDto>> GetAllOrdersAsync(DateTime? startDate = null, DateTime? endDate = null);
}
