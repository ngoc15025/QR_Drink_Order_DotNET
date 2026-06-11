using System.ComponentModel.DataAnnotations;

namespace QRDrinkOrder.Shared.DTOs.Requests;

public class CreateOrderRequest
{
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Số bàn là bắt buộc.")]
    public string TableNumber { get; set; } = string.Empty;

    public string? CouponCode { get; set; }

    public int? PointsToUse { get; set; }

    public string? Note { get; set; }

    [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc.")]
    public byte PaymentMethod { get; set; } // 0: Tiền mặt, 1: SePay VietQR

    [Required(ErrorMessage = "Giỏ hàng không được trống.")]
    [MinLength(1, ErrorMessage = "Giỏ hàng phải có ít nhất một món.")]
    public List<OrderItemRequest> Items { get; set; } = new();

    public int? EmployeeId { get; set; } // Nếu nhân viên đặt món hộ
    
    public bool UseEmployeeBenefit { get; set; } // Nhân viên dùng đặc quyền giảm 50%
}

public class OrderItemRequest
{
    [Required]
    public int DrinkId { get; set; }

    [Required]
    [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100.")]
    public int Quantity { get; set; }

    public byte? SweetnessLevel { get; set; } // 0: 0% đường, 1: 50%, 2: 100%

    public byte? IceLevel { get; set; }       // 0: 0% đá, 1: 50%, 2: 100%

    public int? SizeId { get; set; }

    public List<int> ToppingIds { get; set; } = new();

    public string? ItemNote { get; set; }     // Ghi chú món

    [Required]
    public decimal UnitPrice { get; set; }
}

public class UpdateOrderStatusRequest
{
    [Required]
    public byte OrderStatus { get; set; } // 0: Chờ, 1: Đang chuẩn bị, 2: Hoàn thành, 3: Hủy
}

public class ConfirmPaymentRequest
{
    public string? TransactionId { get; set; }
}
