using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace QRDrinkOrder.API.Hubs;

public class OrderHub : Hub
{
    // Tham gia nhóm cho Nhân viên (để nhận đơn mới)
    [Authorize(Roles = "Employee")]
    public async Task JoinStaffGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Staff");
    }

    // Tham gia nhóm cho Khách hàng cụ thể theo SessionId để nhận cập nhật đơn hàng riêng biệt
    public async Task JoinCustomerSessionGroup(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Customer_{sessionId}");
    }

    // Gửi thông báo có đơn hàng mới đã thanh toán/được tạo tới toàn bộ nhân viên
    public async Task NotifyNewOrder(object orderDto)
    {
        await Clients.Group("Staff").SendAsync("ReceiveNewOrder", orderDto);
    }

    // Gửi cập nhật trạng thái đơn hàng đến khách hàng
    public async Task NotifyOrderStatusChanged(string sessionId, int orderId, byte status, string statusName)
    {
        await Clients.Group($"Customer_{sessionId}").SendAsync("ReceiveStatusUpdate", new { OrderId = orderId, Status = status, StatusName = statusName });
        // Đồng thời cập nhật bảng điều khiển Kanban của nhân viên
        await Clients.Group("Staff").SendAsync("ReceiveStatusUpdateAtPOS", new { OrderId = orderId, Status = status, StatusName = statusName });
    }
}
