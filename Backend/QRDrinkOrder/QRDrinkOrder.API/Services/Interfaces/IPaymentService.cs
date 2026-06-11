namespace QRDrinkOrder.API.Services.Interfaces;

public interface IPaymentService
{
    Task<bool> ProcessSePayWebhookAsync(string content, decimal amount, string transactionId, string accountNumber);
    Task<bool> ConfirmCashPaymentAsync(int orderId, int employeeId);
}
