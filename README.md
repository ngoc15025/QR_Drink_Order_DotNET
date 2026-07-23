# ☕ Ngoc UwU Coffee - QR Drink Order System

<p align="center">
<img width="225" height="225" alt="qr-code" src="https://github.com/user-attachments/assets/3ccf0af3-39dd-4072-91c7-163c1d35b946" />

  <br>
  <sub><b>Quét mã QR trên để trải nghiệm ứng dụng đặt đồ uống tại bàn</b></sub>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 9" />
  <img src="https://img.shields.io/badge/Blazor-WebAssembly-512BD4?style=for-the-badge&logo=blazor&logoColor=white" alt="Blazor WebAssembly" />
  <img src="https://img.shields.io/badge/ASP.NET_Core-Web_API-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="ASP.NET Core Web API" />
  <img src="https://img.shields.io/badge/SignalR-Real--time-NET?style=for-the-badge&logo=dotnet&logoColor=white" alt="SignalR" />
  <img src="https://img.shields.io/badge/SePay-VietQR_Automation-0066FF?style=for-the-badge" alt="SePay" />
  <img src="https://img.shields.io/badge/SQL_Server-Database-CC292B?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" alt="SQL Server" />
</p>

---

## 📌 Giới thiệu dự án

**Ngoc UwU Coffee (QR Drink Order System)** là hệ thống đặt đồ uống và món ăn kèm thông minh qua mã QR tại bàn dành cho các mô hình quán Cà phê / Trà sữa / Đồ uống hiện đại. Hệ thống được nghiên cứu và phát triển trong khuôn khổ **Luận văn tốt nghiệp**, giải quyết luồng phục vụ tự động hóa khép kín: từ khâu quét QR gọi món, gợi ý sản phẩm thông minh theo thời tiết (AI Recommendation), tùy chỉnh ly nước linh hoạt, áp dụng mã giảm giá, thanh toán chuyển khoản tự động (SePay Webhook) đến quy trình xử lý đơn hàng thời gian thực (SignalR) và quản lý bán hàng (POS).

---

## 🌟 Các tính năng cốt lõi

### 📱 1. Dành cho Khách hàng (Customer Web App)
- 🔗 **Quét mã QR truy cập nhanh:** Khách hàng quét mã QR tại bàn để truy cập thẳng vào giao diện Menu trực tuyến mà không cần tải ứng dụng.
- 🌤️ **Gợi ý món ăn theo thời tiết (AI Recommendation):** Tự động phân tích nhiệt độ thực tế tại quán qua OpenWeather API để đề xuất các thức uống phù hợp (Đồ uống lạnh/Trà trái cây khi trời nóng, Cà phê nóng khi trời lạnh).
- 🧁 **Gợi ý món ăn kèm hợp vị (Smart Pairing & Modal Stacking):** Tự động đề xuất các loại bánh ngọt/snack tương thích với đồ uống đã chọn. Áp dụng kiến trúc **Modal Stacking (`Stack<DrinkModalState>`)** cho phép mở modal chỉnh món ăn kèm đè lên modal món chính mà **không làm mất** cấu hình ly (Size, Đường, Đá, Topping) đã thiết lập trước đó.
- 💳 **Thanh toán VietQR tự động hóa (SePay Integration):**
  - Khởi tạo mã VietQR động đếm ngược 15 phút với số tiền và nội dung chuyển khoản đã được điền sẵn chính xác.
  - Tự động xác nhận thanh toán thành công trong **2 - 5 giây** qua **SePay Webhook** và cập nhật giao diện điện thoại tức thì bằng **SignalR** mà không cần reload hay nhấn nút thủ công.
- 🎁 **Định danh và Tích điểm thành viên (Loyalty & Coupon):** Định danh bằng Số điện thoại + Xác nhận mã PIN 4 chữ số, tích lũy điểm thưởng tự động và sử dụng Voucher/Coupon giảm giá.
- 🛵 **Chọn hình thức phục vụ:** Linh hoạt lựa chọn **Uống tại bàn** hoặc **Mang đi (Takeaway)**.
- ⏳ **Tra cứu tiến trình & Đánh giá:** Theo dõi tiến độ pha chế thời gian thực (`Đã nhận đơn` ➔ `Đang pha chế` ➔ `Hoàn thành`) và gửi Đánh giá (Số sao + Bình luận + Hình ảnh chụp tại bàn).

### 🖥️ 2. Dành cho Nhân viên (Employee / POS Dashboard)
- 🔔 **Nhận đơn thời gian thực (SignalR Real-time):** Màn hình POS tự động phát âm thanh thông báo "Ting ting" và đẩy thẻ đơn hàng mới vào danh sách chờ xử lý ngay khi khách thanh toán thành công.
- 📑 **Quản lý luồng pha chế & In hóa đơn:** Chuyển trạng thái đơn hàng (Đang pha chế ➔ Hoàn thành), hỗ trợ bấm **In hóa đơn thanh toán** trực tiếp ra máy in nhiệt.
- 🛒 **Đặt món hộ khách (POS Quầy):** Hỗ trợ khách hàng lớn tuổi hoặc người không sử dụng smartphone đặt món và thanh toán trực tiếp tại quầy.
- 🚫 **Bật/Tắt trạng thái món nhanh:** Gạt công tắc tạm ẩn món khi quầy bar hết nguyên liệu, tránh tình trạng khách gọi món không còn khả dụng.

### 👑 3. Dành cho Quản lý & Quản trị viên (Manager & Admin Panel)
- 📊 **Dashboard Báo cáo và Thống kê:** Biểu đồ trực quan hóa tổng doanh thu, số lượng đơn hàng, Top 5 món bán chạy và biểu đồ mật độ khách hàng theo khung giờ.
- 🍹 **Quản lý Thực đơn và Danh mục:** Thêm/Sửa/Xóa mềm (Soft Delete) các món nước và danh mục đồ uống, tích hợp Cloudinary API tải ảnh trực tiếp.
- 🎟️ **Quản lý Mã giảm giá (Coupon):** Thiết lập chiến dịch khuyến mãi (Giảm %, giảm tiền mặt, thời hạn sử dụng, giới hạn lượt dùng theo SĐT).
- 🏦 **Cấu hình Hệ thống và VietQR:** Cài đặt thông số tài khoản ngân hàng thụ hưởng dòng tiền SePay linh hoạt mà không cần can thiệp lại mã nguồn.
- 👥 **Quản lý Nhân sự và Phân quyền (JWT):** Quản lý danh sách tài khoản nhân viên, phân quyền hạn và ghi nhận lịch sử **Audit Logs** tự động cho các thao tác.

---

## 🛠️ Công nghệ sử dụng (Tech Stack)

### **Frontend (Client)**
- **Framework:** Blazor WebAssembly (.NET 9)
- **UI/UX Design:** Glassmorphism Style, Vanilla CSS, Responsive Layout (Inspired by *The Coffee House*)
- **State Management & Communication:** `CartState`, `OrderStateService`, `ModalStack`, JSInterop
- **Auth:** Custom `AuthenticationStateProvider` (JWT) & Firebase Auth (PIN Verification, Tạm thời chưa sử dụng)

### **Backend (API)**
- **Framework:** ASP.NET Core 9 Web API
- **Real-time Communication:** ASP.NET Core SignalR (`OrderHub`)
- **Database & ORM:** SQL Server, Entity Framework Core 9 (với `AuditLogInterceptor` tự động ghi vết)
- **Security & Performance:** JWT Bearer Authentication, Rate Limiting (Fixed Window Limiter), Global Exception Middleware

### **Third-Party Services & APIs**
- **SePay API:** Cổng xử lý Webhook xác thực giao dịch chuyển khoản VietQR tự động.
- **Cloudinary API:** Lưu trữ và tối ưu hóa hình ảnh đồ uống và hình ảnh review từ khách hàng.
- **OpenWeatherMap API:** Cung cấp dữ liệu nhiệt độ/thời tiết thực tế phục vụ AI Recommendation.

---

## 📂 Kiến trúc cấu trúc thư mục

```
QR_Drink_Order_DotNET/
├── qrcode.png                     # Mã QR trải nghiệm ứng dụng
├── Database/                      # Kịch bản SQL Server (Tạo bảng, Seed Menu, Promotions)
│   ├── QRDrinkOrderDBDeploy.sql
│   ├── SeedMenu.sql
│   └── seed_promotions.sql
└── QRDrinkOrder/                  # Solution chính (.NET 9)
    ├── QRDrinkOrder.API/          # ASP.NET Core Web API, Controllers, SignalR Hubs, Services
    │   ├── Controllers/           # Auth, Menu, Order, Payment (SePay Webhook), Review, Report...
    │   ├── Hubs/                  # OrderHub (SignalR Real-time)
    │   ├── Models/                # EF Core Entities & DbContext
    │   ├── Services/              # AiRecommendation, Weather, Payment, Order...
    │   └── Program.cs             # Cấu hình Services, JWT, CORS, Rate Limiter
    ├── QRDrinkOrder.Client/       # Blazor WebAssembly Frontend
    │   ├── Pages/                 # Home, Menu, Cart, OrderTracking, POS, Admin Dashboards...
    │   ├── Services/              # ApiClients, State Management, LocalStorage
    │   └── Layout/                # MainLayout, AdminLayout, Off-canvas Navigation
    └── QRDrinkOrder.Shared/       # Thư viện dùng chung (DTOs, Enums, Constants, Helpers)
```

---

## 🚀 Hướng dẫn Cài đặt & Khởi chạy (Local Development)

### **1. Yêu cầu môi trường**
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (SQL Server Express hoặc LocalDB)
- Visual Studio 2022 (phiên bản 17.8 trở lên) hoặc Visual Studio Code.

### **2. Cấu hình Cơ sở dữ liệu**
1. Mở SQL Server Management Studio (SSMS) hoặc Azure Data Studio.
2. Chạy file SQL `Database/QRDrinkOrderDBDeploy.sql` để khởi tạo database `QRDrinkOrderDB`.
3. Chạy thêm `Database/SeedMenu.sql` và `Database/seed_promotions.sql` để nạp dữ liệu mẫu ban đầu.

### **3. Cấu hình Backend (`QRDrinkOrder.API`)**
Mở file `QRDrinkOrder/QRDrinkOrder.API/appsettings.json` và cập nhật các chuỗi cấu hình:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=QRDrinkOrderDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "SecretKey": "YOUR_SUPER_SECRET_KEY_AT_LEAST_32_BYTES_LONG",
    "Issuer": "QRDrinkOrderAPI",
    "Audience": "QRDrinkOrderClient"
  },
  "SePay": {
    "ApiKey": "YOUR_SEPAY_API_KEY"
  },
  "Cloudinary": {
    "CloudName": "YOUR_CLOUD_NAME",
    "ApiKey": "YOUR_API_KEY",
    "ApiSecret": "YOUR_API_SECRET"
  }
}
```

### **4. Chạy ứng dụng**
Mở Terminal tại thư mục chứa solution `QRDrinkOrder`:

**Khởi chạy Backend API:**
```bash
cd QRDrinkOrder/QRDrinkOrder.API
dotnet run
```
*API sẽ chạy tại đường dẫn mặc định:* `http://localhost:5153` *(Swagger UI mở tại:* `http://localhost:5153/swagger`*)*.

**Khởi chạy Frontend Blazor Client:**
```bash
cd QRDrinkOrder/QRDrinkOrder.Client
dotnet run
```
*Client sẽ chạy tại đường dẫn:* `http://localhost:5242` *(hoặc xem cổng hiển thị trong Terminal)*.

---

## 🎓 Thông tin Đồ án & Tác giả

- **Đồ án:** Luận văn Tốt nghiệp Đại học ngành Công nghệ Thông tin / Kỹ thuật Phần mềm
- **Tên đề tài:** Phát triển hệ thống quản lý và đặt món thông minh cho quán cà phê.
- **Công nghệ chính:** .NET 9, ASP.NET Core Web API, Blazor WebAssembly, SignalR, SePay VietQR.

---
*Phát triển với tất cả tâm huyết cho Luận văn tốt nghiệp.* ❤️
