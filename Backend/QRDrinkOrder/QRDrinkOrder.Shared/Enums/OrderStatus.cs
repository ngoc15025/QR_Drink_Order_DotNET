namespace QRDrinkOrder.Shared.Enums;

public enum OrderStatus : byte
{
    PendingPayment = 0, // Chờ thanh toán
    Preparing = 1,      // Đang chuẩn bị
    Completed = 2,      // Hoàn thành
    Cancelled = 3       // Đã hủy
}
