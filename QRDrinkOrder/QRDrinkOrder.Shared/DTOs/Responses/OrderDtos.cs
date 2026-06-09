namespace QRDrinkOrder.Shared.DTOs.Responses;

public class OrderDto
{
    public int OrderId { get; set; }
    public Guid SessionId { get; set; }
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? TableNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public byte OrderStatus { get; set; } // 0: Chờ thanh toán, 1: Đang chuẩn bị, 2: Hoàn thành, 3: Đã hủy
    public string OrderStatusName { get; set; } = string.Empty;
    public int? CouponId { get; set; }
    public string? CouponCode { get; set; }
    public int? PointsUsed { get; set; }
    public DateTime OrderDate { get; set; }
    public string? Note { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public PaymentDto? Payment { get; set; }
    public string? CustomerPhone { get; set; }
    public bool IsReviewed { get; set; }
}

public class OrderItemDto
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public int DrinkId { get; set; }
    public string DrinkName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public byte? SweetnessLevel { get; set; }
    public byte? IceLevel { get; set; }
    public string? SizeName { get; set; }
    public List<string> ToppingNames { get; set; } = new();
    public string? ItemNote { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
}

public class PaymentDto
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public byte PaymentMethod { get; set; } // 0: Tiền mặt, 1: SePay
    public string PaymentMethodName { get; set; } = string.Empty;
    public byte PaymentStatus { get; set; } // 0: Chờ, 1: Thành công, 2: Thất bại
    public string PaymentStatusName { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime? PaidAt { get; set; }
}
