namespace QRDrinkOrder.Shared.Enums;

public enum PaymentStatus : byte
{
    Pending = 0, // Đang chờ
    Success = 1, // Thành công
    Failed = 2   // Thất bại
}
