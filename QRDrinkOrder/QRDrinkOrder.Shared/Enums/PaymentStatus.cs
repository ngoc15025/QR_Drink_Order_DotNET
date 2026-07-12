namespace QRDrinkOrder.Shared.Enums;

public enum PaymentStatus : byte
{
    Pending = 0, // Đang chờ
    Success = 1, // Thành công
    Failed = 2,  // Thất bại
    Refunded = 3 // Đã hoàn tiền (Hoàn trả do hủy đơn)
}
