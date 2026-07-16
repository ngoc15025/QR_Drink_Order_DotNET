namespace QRDrinkOrder.API.Models;

public partial class OrderItemTopping
{
    public int OrderItemId { get; set; }
    public int ToppingId { get; set; }

    public virtual OrderItem OrderItem { get; set; } = null!;
    public virtual Topping Topping { get; set; } = null!;
}
