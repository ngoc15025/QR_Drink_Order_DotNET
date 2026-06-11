namespace QRDrinkOrder.API.Models;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public int DrinkId { get; set; }

    public int Quantity { get; set; }

    public byte? SweetnessLevel { get; set; }

    public byte? IceLevel { get; set; }

    public string? ItemNote { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal? SubTotal { get; set; }

    public int? SizeId { get; set; }

    public virtual Drink Drink { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual Size? Size { get; set; }

    public virtual ICollection<OrderItemTopping> OrderItemToppings { get; set; } = new List<OrderItemTopping>();
}
