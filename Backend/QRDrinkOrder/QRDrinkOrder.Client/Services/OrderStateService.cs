namespace QRDrinkOrder.Client.Services
{
    public enum OrderType
    {
        DineIn, // Tại bàn
        Pickup  // Nhận tại quầy
    }

    public class OrderStateService
    {
        private OrderType _currentOrderType = OrderType.DineIn;
        private string _tableNumber = string.Empty;

        public event Action? OnStateChanged;

        public OrderType CurrentOrderType
        {
            get => _currentOrderType;
            set
            {
                if (_currentOrderType != value)
                {
                    _currentOrderType = value;
                    NotifyStateChanged();
                }
            }
        }

        public string TableNumber
        {
            get => _tableNumber;
            set
            {
                if (_tableNumber != value)
                {
                    _tableNumber = value;
                    NotifyStateChanged();
                }
            }
        }

        private void NotifyStateChanged() => OnStateChanged?.Invoke();
    }
}
