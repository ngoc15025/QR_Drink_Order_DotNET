using QRDrinkOrder.Shared.DTOs.Requests;
using QRDrinkOrder.Shared.DTOs.Responses;
using System.Net.Http.Json;

namespace QRDrinkOrder.Client.Services;

public class OrderApiClient
{
    private readonly HttpClient _httpClient;

    public OrderApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<OrderDto?> CreateOrderAsync(Guid sessionId, CreateOrderRequest request)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/orders");
        httpRequest.Headers.Add("X-Session-ID", sessionId.ToString());
        httpRequest.Content = JsonContent.Create(request);

        var response = await _httpClient.SendAsync(httpRequest);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<OrderDto>();
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        try
        {
            var errorJson = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(errorContent);
            if (errorJson.TryGetProperty("message", out var msg) || errorJson.TryGetProperty("Message", out msg))
            {
                throw new Exception(msg.GetString());
            }
        }
        catch (System.Text.Json.JsonException) { }
        
        throw new Exception(errorContent);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
    {
        return await _httpClient.GetFromJsonAsync<OrderDto>($"api/orders/{orderId}");
    }

    public async Task<List<OrderDto>> GetActiveOrdersAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<OrderDto>>("api/orders/active") ?? new List<OrderDto>();
    }

    public async Task<List<OrderDto>> GetOrderHistoryByPhoneAsync(string phone)
    {
        return await _httpClient.GetFromJsonAsync<List<OrderDto>>($"api/orders/history?phone={phone}") ?? new List<OrderDto>();
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, byte status)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/orders/{orderId}/status", new UpdateOrderStatusRequest { OrderStatus = status });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CancelOrderAsync(int orderId, Guid sessionId)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"api/orders/{orderId}/cancel");
        httpRequest.Headers.Add("X-Session-ID", sessionId.ToString());

        var response = await _httpClient.SendAsync(httpRequest);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdatePaymentMethodAsync(int orderId, Guid sessionId, byte paymentMethod)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"api/orders/{orderId}/payment-method");
        httpRequest.Headers.Add("X-Session-ID", sessionId.ToString());
        httpRequest.Content = JsonContent.Create(paymentMethod);

        var response = await _httpClient.SendAsync(httpRequest);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ConfirmCashPaymentAsync(int orderId)
    {
        var response = await _httpClient.PostAsync($"api/payments/{orderId}/confirm-cash", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<OrderDto>> GetAllOrdersAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var url = "api/orders";
        var queryParams = new List<string>();

        if (startDate.HasValue)
            queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");

        if (endDate.HasValue)
            queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

        if (queryParams.Count > 0)
            url += "?" + string.Join("&", queryParams);

        return await _httpClient.GetFromJsonAsync<List<OrderDto>>(url) ?? new List<OrderDto>();
    }
}
