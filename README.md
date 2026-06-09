# ☕ Ngoc UwU Coffee - QR Drink Order System

**Dự án Luận văn tốt nghiệp** xây dựng hệ thống đặt đồ uống qua mã QR tại bàn, tích hợp thanh toán tự động và hệ thống quản lý thời gian thực dành cho quán cà phê.

## 🌟 Tính năng chính

### 📱 Dành cho Khách hàng (Customer)
- **Đặt món tại bàn:** Quét mã QR để xem menu và gọi món trực tiếp.
- **Thanh toán tự động:** Quét mã QR thanh toán ngân hàng (Tích hợp **SePay Webhook**).
- **Trải nghiệm mượt mà:** Giao diện tối ưu di động (Lấy cảm hứng từ The Coffee House). Tính năng đếm ngược hủy đơn, đổi phương thức thanh toán.
- **Lịch sử & Đánh giá:** Tra cứu lịch sử đơn hàng và để lại review.

### 💼 Dành cho Nhân viên (Employee / POS)
- **Thông báo Real-time:** Nhận đơn hàng ngay lập tức không cần tải lại trang (nhờ **SignalR**).
- **Hệ thống POS:** Quản lý luồng đơn hàng (Chuẩn bị, Đã phục vụ), hỗ trợ in hóa đơn trực tiếp.

### ⚙️ Dành cho Quản lý (Manager & Admin)
- **UI/UX Hiện đại:** Giao diện quản trị phong cách **Glassmorphism**, thanh điều hướng Off-canvas thông minh cho màn hình điện thoại.
- **Quản lý đa năng:** Kiểm soát Menu (hỗ trợ phân loại đồ uống Hot/Iced phục vụ AI gợi ý sau này), Mã giảm giá (Coupon), Chương trình khuyến mãi, và Tài khoản.

## 🛠️ Công nghệ sử dụng (Tech Stack)

- **Frontend:** Blazor WebAssembly (.NET 9), CSS thuần (thiết kế đáp ứng đa nền tảng).
- **Backend:** ASP.NET Core Web API (.NET 9), SignalR.
- **Database:** Entity Framework Core, SQL Server (Sẵn sàng mở rộng sang PostgreSQL/Supabase).
- **Thanh toán:** SePay API.

## 🚀 Hướng dẫn cài đặt & Chạy dự án (Local)

### Yêu cầu hệ thống:
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server Local (hoặc thay đổi chuỗi kết nối tùy ý).

### Các bước thiết lập:
1. **Clone dự án về máy:**
   ```bash
   git clone <url-repo-của-bạn>
   ```
2. **Cấu hình Backend (`QRDrinkOrder.API`):**
   - Đổi tên file `appsettings.example.json` thành `appsettings.json`.
   - Cập nhật Chuỗi kết nối Database (`DefaultConnection`), Khóa bí mật (`Jwt:SecretKey`), và thông tin Vapid.
   - Cập nhật Database:
     ```bash
     cd Backend/QRDrinkOrder/QRDrinkOrder.API
     dotnet ef database update
     ```
3. **Khởi động Server:**
   - Chạy Backend (API):
     ```bash
     dotnet run --project Backend/QRDrinkOrder/QRDrinkOrder.API
     ```
   - Chạy Frontend (Client):
     ```bash
     dotnet run --project Backend/QRDrinkOrder/QRDrinkOrder.Client
     ```

## 🏗️ Kiến trúc thư mục
- 📂 `Backend/QRDrinkOrder/`
  - `QRDrinkOrder.API/`: Chứa mã nguồn Backend API, SignalR Hubs và Controller xử lý Webhook.
  - `QRDrinkOrder.Client/`: Giao diện ứng dụng viết bằng Blazor WebAssembly.
  - `QRDrinkOrder.Shared/`: Chứa các Entity, Models và DTOs dùng chung cho cả Client và API.

## 📌 Lộ trình tương lai
- ☁️ Triển khai ứng dụng lên Cloud (Vercel, Render, Supabase).
- 🤖 Tích hợp hệ thống AI gợi ý đồ uống thông minh dựa vào điều kiện thời tiết và dữ liệu nhiệt độ món (Hot/Iced/Both).

---
*Phát triển cho đồ án Luận văn tốt nghiệp.* ❤️
