# ☕ Ngoc UwU Coffee - QR Drink Order System

**Dự án Luận văn tốt nghiệp** xây dựng hệ thống đặt đồ uống qua mã QR tại bàn, tích hợp thanh toán tự động và hệ thống quản lý thời gian thực dành cho quán cà phê.

## 🌟 Tính năng chính

### 📱 Dành cho Khách hàng (Customer)
- **Đặt món tại bàn:** Quét mã QR để xem menu và gọi món trực tiếp.
- **Thanh toán tự động:** Quét mã QR thanh toán ngân hàng (Tích hợp **SePay Webhook**).
- **Trải nghiệm mượt mà:** Giao diện tối ưu di động (Lấy cảm hứng từ The Coffee House). Tính năng đếm ngược hủy đơn, đổi phương thức thanh toán.
- **Lịch sử và Đánh giá:** Tra cứu lịch sử đơn hàng và để lại review.

### 💼 Dành cho Nhân viên (Employee)
- **Thông báo Real-time:** Nhận đơn hàng ngay lập tức không cần tải lại trang (nhờ **SignalR**).
- **Hệ thống POS:** Quản lý luồng đơn hàng (Đang chuẩn bị, Đã hoàn thành), hỗ trợ in hóa đơn trực tiếp.

### ⚙️ Dành cho Quản lý (Manager & Admin)
- **UI/UX Hiện đại:** Giao diện quản trị phong cách **Glassmorphism**, thanh điều hướng Off-canvas thông minh cho màn hình điện thoại.
- **Quản lý đa năng:** Quản lý Menu, Mã giảm giá (Coupon), Chương trình khuyến mãi và Tài khoản.

## 🛠️ Công nghệ sử dụng (Tech Stack)

- **Frontend:** Blazor WebAssembly (.NET 9), CSS thuần.
- **Backend:** ASP.NET Core Web API (.NET 9), SignalR.
- **Database:** Entity Framework Core, SQL Server.
- **Thanh toán:** SePay API.

## 🚀 Hướng dẫn cài đặt & Chạy dự án (Local)

## 🏗️ Kiến trúc thư mục
- 📂 `Backend/QRDrinkOrder/`
  - `QRDrinkOrder.API/`: Chứa mã nguồn Backend API, SignalR Hubs và Controller xử lý Webhook.
  - `QRDrinkOrder.Client/`: Giao diện ứng dụng viết bằng Blazor WebAssembly.
  - `QRDrinkOrder.Shared/`: Chứa các Entity, Models và DTOs dùng chung cho cả Client và API.

*Phát triển cho đồ án Luận văn tốt nghiệp.* ❤️
