using System.Text.Json;
using Microsoft.JSInterop;
using QRDrinkOrder.Shared.DTOs.Responses;

namespace QRDrinkOrder.Client.Services.State;

public class CartState
{
    private readonly IJSRuntime _jsRuntime;
    private const string CartStorageKey = "cartItems";

    public Guid SessionId { get; private set; }
    public List<CartItem> Items { get; private set; } = new();

    public event Action? OnChange;

    public CartState(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        // Thử lấy SessionId từ localStorage qua JS Interop
        try
        {
            var storedSessionId = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "sessionId");
            if (!string.IsNullOrEmpty(storedSessionId) && Guid.TryParse(storedSessionId, out Guid id))
            {
                SessionId = id;
            }
            else
            {
                SessionId = Guid.NewGuid();
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "sessionId", SessionId.ToString());
            }

            // Khôi phục danh sách giỏ hàng từ localStorage khi F5 reload trang
            var storedCartJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", CartStorageKey);
            if (!string.IsNullOrEmpty(storedCartJson))
            {
                var restoredItems = JsonSerializer.Deserialize<List<CartItem>>(storedCartJson);
                if (restoredItems != null)
                {
                    Items = restoredItems;
                }
            }
        }
        catch
        {
            if (SessionId == Guid.Empty)
                SessionId = Guid.NewGuid();
        }
    }

    private async Task SaveCartAsync()
    {
        try
        {
            if (Items == null || !Items.Any())
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", CartStorageKey);
            }
            else
            {
                var json = JsonSerializer.Serialize(Items);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", CartStorageKey, json);
            }
        }
        catch
        {
            // Bỏ qua lỗi lưu localStorage nếu bị chặn bộ nhớ
        }
    }

    public void AddToCart(DrinkDto drink, int quantity, byte? sweetness, byte? ice, string? note, SizeDto? size, List<ToppingDto> toppings)
    {
        var existingItem = Items.FirstOrDefault(i =>
            i.Drink.DrinkId == drink.DrinkId &&
            i.SweetnessLevel == sweetness &&
            i.IceLevel == ice &&
            i.ItemNote == note &&
            i.SelectedSize?.SizeId == size?.SizeId &&
            i.SelectedToppings.Select(t => t.ToppingId).OrderBy(id => id).SequenceEqual(toppings.Select(t => t.ToppingId).OrderBy(id => id))
        );

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            Items.Add(new CartItem
            {
                Drink = drink,
                Quantity = quantity,
                SweetnessLevel = sweetness,
                IceLevel = ice,
                ItemNote = note,
                SelectedSize = size,
                SelectedToppings = toppings.ToList()
            });
        }
        _ = SaveCartAsync();
        NotifyStateChanged();
    }

    public void UpdateQuantity(CartItem item, int newQuantity)
    {
        if (newQuantity <= 0)
        {
            RemoveItem(item);
        }
        else
        {
            item.Quantity = newQuantity;
            _ = SaveCartAsync();
            NotifyStateChanged();
        }
    }

    public void RemoveItem(CartItem item)
    {
        Items.Remove(item);
        _ = SaveCartAsync();
        NotifyStateChanged();
    }

    public void ClearCart()
    {
        Items.Clear();
        _ = SaveCartAsync();
        NotifyStateChanged();
    }

    public decimal GetTotalAmount()
    {
        return Items.Sum(i => i.TotalPrice);
    }

    public int GetTotalQuantity()
    {
        return Items.Sum(i => i.Quantity);
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

public class CartItem
{
    public DrinkDto Drink { get; set; } = new();
    public int Quantity { get; set; }
    public byte? SweetnessLevel { get; set; }
    public byte? IceLevel { get; set; }
    public string? ItemNote { get; set; }
    public SizeDto? SelectedSize { get; set; }
    public List<ToppingDto> SelectedToppings { get; set; } = new();

    public decimal UnitPrice => Drink.BasePrice + (SelectedSize?.PriceOffset ?? 0) + SelectedToppings.Sum(t => t.Price);
    public decimal TotalPrice => UnitPrice * Quantity;
}
