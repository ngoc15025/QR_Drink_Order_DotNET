-- ========================================================
-- DATABASE: QR Drink Order (Chuẩn hóa 3NF - Cập nhật mới nhất)
-- NỀN TẢNG: SQL SERVER
-- ========================================================

CREATE DATABASE QRDrinkOrderDB;
GO

USE QRDrinkOrderDB;
GO

-- ==========================================
-- PHÂN HỆ 1: QUẢN TRỊ & NHÂN SỰ (IDENTITY)
-- ==========================================

-- 1. Bảng Vai trò (Roles)
CREATE TABLE Roles (
    RoleID TINYINT PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL 
);

-- 2. Bảng Tài khoản (Accounts)
CREATE TABLE Accounts (
    AccountID INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(255) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    RoleID TINYINT NOT NULL,
    IsActive BIT DEFAULT 1,
    IsDeleted BIT DEFAULT 0, 
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);

-- 3. Bảng Quản lý (Managers)
CREATE TABLE Managers (
    ManagerID INT PRIMARY KEY IDENTITY(1,1),
    AccountID INT UNIQUE NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(15),
    FOREIGN KEY (AccountID) REFERENCES Accounts(AccountID)
);

-- 4. Bảng Nhân viên (Employees)
CREATE TABLE Employees (
    EmployeeID INT PRIMARY KEY IDENTITY(1,1),
    AccountID INT UNIQUE NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(15),
    FOREIGN KEY (AccountID) REFERENCES Accounts(AccountID)
);

-- ==========================================
-- PHÂN HỆ 2: THỰC ĐƠN & ĐA NGÔN NGỮ
-- ==========================================

-- 5. Bảng Danh mục gốc (Categories)
CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    DisplayOrder INT DEFAULT 0, 
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- 6. Bảng Bản dịch Danh mục (CategoryTranslations)
CREATE TABLE CategoryTranslations (
    CategoryTranslationID INT PRIMARY KEY IDENTITY(1,1),
    CategoryID INT NOT NULL,
    LanguageCode CHAR(2) NOT NULL, 
    CategoryName NVARCHAR(100) NOT NULL, 
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID) ON DELETE CASCADE
);

-- 7. Bảng Món nước (Drinks)
CREATE TABLE Drinks (
    DrinkID INT PRIMARY KEY IDENTITY(1,1),
    CategoryID INT NOT NULL,
    ImageUrl NVARCHAR(MAX) NOT NULL,
    BasePrice DECIMAL(18, 2) NOT NULL,
    TemperatureType TINYINT NULL, -- 0: Nóng, 1: Lạnh, 2: Tùy chọn, 3: Bánh/Khác
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);

-- 8. Bảng Bản dịch Món nước (DrinkTranslations)
CREATE TABLE DrinkTranslations (
    TranslationID INT PRIMARY KEY IDENTITY(1,1),
    DrinkID INT NOT NULL,
    LanguageCode CHAR(2) NOT NULL, 
    DrinkName NVARCHAR(150) NOT NULL,
    Description NVARCHAR(MAX),
    FOREIGN KEY (DrinkID) REFERENCES Drinks(DrinkID) ON DELETE CASCADE
);

-- ==========================================
-- PHÂN HỆ 3: GIAO DỊCH & KHUYẾN MÃI
-- ==========================================

-- 9. Bảng Mã giảm giá (Coupons)
-- Cập nhật: Bảng Mã giảm giá (Coupons) - Kiến trúc Voucher F&B Thực tế
CREATE TABLE Coupons (
    CouponID INT PRIMARY KEY IDENTITY(1,1),
    CouponCode NVARCHAR(50) UNIQUE NOT NULL,
    -- 1. KIỂU GIẢM GIÁ
    DiscountType TINYINT NOT NULL, -- 0: Trừ tiền mặt (Fixed), 1: Giảm phần trăm (%)
    DiscountValue DECIMAL(18, 2) NOT NULL, -- Giá trị giảm (Ví dụ: 20000 hoặc 15)
    -- 2. ĐIỀU KIỆN VÀ GIỚI HẠN (Phòng thủ ngân sách)
    MinOrderValue DECIMAL(18, 2) DEFAULT 0, -- Đơn tối thiểu để áp dụng (VD: Đơn từ 100k)
    MaxDiscountAmount DECIMAL(18, 2) NULL, -- Giảm tối đa (Dành cho loại % - VD: Tối đa 30k)
    -- 3. KIỂU SOÁT SỐ LƯỢNG
    UsageLimit INT NULL, -- Tổng số lượng mã phát ra (NULL nếu không giới hạn)
    UsedCount INT DEFAULT 0, -- Số lượng mã khách đã dùng thành công
    -- 4. THỜI GIAN
    StartDate DATETIME2 NOT NULL DEFAULT GETDATE(), -- Thời điểm mã bắt đầu có hiệu lực
    EndDate DATETIME2 NOT NULL, -- Thời điểm mã hết hạn
    IsActive BIT DEFAULT 1
);

-- 10. Bảng Quản lý Chiến dịch / Tin tức (Promotions)
-- Dùng để hiển thị Banner ở trang chủ
CREATE TABLE Promotions (
    PromotionID INT PRIMARY KEY IDENTITY(1,1),
    ImageUrl NVARCHAR(MAX) NOT NULL, -- Ảnh Banner quảng cáo
    CouponID INT NULL, -- Liên kết trực tiếp với mã giảm giá (Nếu tin tức này là tặng mã)
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (CouponID) REFERENCES Coupons(CouponID) ON DELETE SET NULL
);

-- 11. Bảng Bản dịch Tin tức Khuyến mãi (PromotionTranslations)
CREATE TABLE PromotionTranslations (
    TranslationID INT PRIMARY KEY IDENTITY(1,1),
    PromotionID INT NOT NULL,
    LanguageCode CHAR(2) NOT NULL, 
    Title NVARCHAR(255) NOT NULL, -- Tiêu đề tin tức (VD: 'Mời bạn mới 50%')
    Content NVARCHAR(MAX), -- Nội dung chi tiết thể lệ chương trình
    FOREIGN KEY (PromotionID) REFERENCES Promotions(PromotionID) ON DELETE CASCADE
);

-- 12. Bảng Phiên khách hàng ẩn danh (CustomerSessions) - BẢO MẬT UUID
CREATE TABLE CustomerSessions (
    SessionID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Phone NVARCHAR(15) NULL, 
    PreferredLanguage CHAR(2) DEFAULT 'vi',
    DeviceInfo NVARCHAR(255) NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    ExpiredAt DATETIME2 NULL
);

-- ==========================================
-- BẢNG TÙY CHỈNH MÓN (KÍCH CỠ & TOPPING)
-- ==========================================

CREATE TABLE Sizes (
    SizeID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL,
    PriceOffset DECIMAL(18, 2) NOT NULL DEFAULT 0,
    IsActive BIT DEFAULT 1
);

CREATE TABLE Toppings (
    ToppingID INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Price DECIMAL(18, 2) NOT NULL DEFAULT 0,
    IsActive BIT DEFAULT 1
);

-- 13. Bảng Đơn hàng (Orders)
CREATE TABLE Orders (
    OrderID INT PRIMARY KEY IDENTITY(1,1),
    SessionID UNIQUEIDENTIFIER NOT NULL,
    EmployeeID INT NULL, 
    TableNumber NVARCHAR(10) NULL,
    TotalAmount DECIMAL(18, 2) NOT NULL,
    DiscountAmount DECIMAL(18, 2) DEFAULT 0,
    FinalAmount AS (TotalAmount - DiscountAmount), -- SQL tự tính toán
    OrderStatus TINYINT DEFAULT 0, -- 0: Chờ thanh toán, 1: Đang chuẩn bị, 2: Hoàn thành, 3: Đã hủy
    CouponID INT NULL,
    PointsUsed INT DEFAULT 0,
    OrderDate DATETIME2 DEFAULT GETDATE(),
    Note NVARCHAR(MAX) NULL,
    FOREIGN KEY (SessionID) REFERENCES CustomerSessions(SessionID),
    FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID),
    FOREIGN KEY (CouponID) REFERENCES Coupons(CouponID)
);

-- 14. Bảng Chi tiết Đơn hàng (OrderItems)
CREATE TABLE OrderItems (
    OrderItemID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT NOT NULL,
    DrinkID INT NOT NULL,
    SizeID INT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    SweetnessLevel TINYINT NULL,
    IceLevel TINYINT NULL,
    ItemNote NVARCHAR(MAX) NULL,
    UnitPrice DECIMAL(18, 2) NOT NULL,
    SubTotal AS (Quantity * UnitPrice), -- SQL tự tính toán
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID),
    FOREIGN KEY (DrinkID) REFERENCES Drinks(DrinkID),
    FOREIGN KEY (SizeID) REFERENCES Sizes(SizeID)
);

-- 14.1. Bảng Chi tiết Topping của Đơn hàng (OrderItemToppings)
CREATE TABLE OrderItemToppings (
    OrderItemID INT NOT NULL,
    ToppingID INT NOT NULL,
    PRIMARY KEY (OrderItemID, ToppingID),
    FOREIGN KEY (OrderItemID) REFERENCES OrderItems(OrderItemID) ON DELETE CASCADE,
    FOREIGN KEY (ToppingID) REFERENCES Toppings(ToppingID)
);

-- 15. Bảng Thanh toán (Payments)
CREATE TABLE Payments (
    PaymentID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT UNIQUE NOT NULL,
    PaymentMethod TINYINT NOT NULL, -- 0: Tiền mặt, 1: SePay VietQR
    PaymentStatus TINYINT DEFAULT 0, -- 0: Đang chờ, 1: Thành công, 2: Thất bại
    TransactionID NVARCHAR(100) NULL, 
    Amount DECIMAL(18, 2) NOT NULL,
    PaidAt DATETIME2 NULL,
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID)
);

-- 16. Bảng Ưu đãi nhân viên (StaffBenefits)
CREATE TABLE StaffBenefits (
    BenefitID INT PRIMARY KEY IDENTITY(1,1),
    EmployeeID INT NOT NULL,
    OrderID INT NOT NULL,
    UsedDate DATE DEFAULT CAST(GETDATE() AS DATE),
    FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID),
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID)
);

-- ==========================================
-- PHÂN HỆ 4: ĐÁNH GIÁ & NHẬT KÝ HỆ THỐNG
-- ==========================================

-- 17. Bảng Nhật ký hệ thống (AuditLogs)
CREATE TABLE AuditLogs (
    LogID INT PRIMARY KEY IDENTITY(1,1),
    AccountID INT NULL,
    Action NVARCHAR(100) NOT NULL,
    TableName NVARCHAR(100) NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (AccountID) REFERENCES Accounts(AccountID)
);

-- 18. Bảng Đánh giá dịch vụ (Reviews)
CREATE TABLE Reviews (
    ReviewID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT UNIQUE NOT NULL, 
    SessionID UNIQUEIDENTIFIER NOT NULL,
    Rating TINYINT NOT NULL CHECK (Rating >= 1 AND Rating <= 5), 
    Comment NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID),
    FOREIGN KEY (SessionID) REFERENCES CustomerSessions(SessionID)
);

-- 19. Bảng Hình ảnh đánh giá (ReviewImages)
CREATE TABLE ReviewImages (
    ImageID INT PRIMARY KEY IDENTITY(1,1),
    ReviewID INT NOT NULL,
    ImageUrl NVARCHAR(MAX) NOT NULL, 
    UploadedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (ReviewID) REFERENCES Reviews(ReviewID) ON DELETE CASCADE 
);

-- 20. Bảng Lịch sử sử dụng Mã giảm giá (CouponUsages)
-- Giải quyết mối quan hệ N-N và cấu hình ràng buộc chặn spam mã theo Số điện thoại
CREATE TABLE CouponUsages (
    UsageID INT PRIMARY KEY IDENTITY(1,1),
    CouponID INT NOT NULL,
    Phone NVARCHAR(15) NOT NULL, -- Số điện thoại thực tế của khách đã nhập mã
    OrderID INT NOT NULL, -- Mã hóa đơn đã áp dụng thành công
    UsedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (CouponID) REFERENCES Coupons(CouponID),
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID)
);
-- TẠO RÀNG BUỘC DUY NHẤT (UNIQUE INDEX MULTI-COLUMN)
-- Đảm bảo: 1 Số điện thoại + 1 Mã giảm giá = Chỉ xuất hiện tối đa 1 dòng dữ liệu trong bảng này
CREATE UNIQUE INDEX UX_Coupon_Phone ON CouponUsages (CouponID, Phone);

-- ==========================================
-- TỐI ƯU HÓA TRUY VẤN (INDEXES)
-- ==========================================

-- Tối ưu tốc độ tra cứu lịch sử mua hàng bằng số điện thoại
CREATE INDEX IX_CustomerSessions_Phone ON CustomerSessions(Phone);

-- Tối ưu tốc độ truy xuất báo cáo doanh thu theo ngày
CREATE INDEX IX_Orders_Date ON Orders(OrderDate);

-- ==========================================
-- PHÂN HỆ 5: CẤU HÌNH & TÍCH ĐIỂM (LOYALTY)
-- ==========================================

-- 21. Bảng Tài khoản Ngân hàng (BankAccounts Admin) 
CREATE TABLE BankAccounts (
    BankAccountId INT PRIMARY KEY IDENTITY(1,1),
    BankCode NVARCHAR(50) NOT NULL,
    AccountNumber NVARCHAR(50) NOT NULL,
    AccountName NVARCHAR(255) NOT NULL,
    IsActive BIT DEFAULT 1
);

-- 22. Bảng Tích điểm Thành viên (Memberships)
CREATE TABLE Memberships (
    MembershipId INT PRIMARY KEY IDENTITY(1,1),
    Phone NVARCHAR(15) NOT NULL UNIQUE,
    Points INT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- 23. Bảng Lịch sử Tích điểm (PointHistories)
CREATE TABLE PointHistories (
    HistoryId INT PRIMARY KEY IDENTITY(1,1),
    Phone NVARCHAR(15) NOT NULL,
    PointsChanged INT NOT NULL,
    Reason NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (Phone) REFERENCES Memberships(Phone)
);

-- 24. Bảng Cấu hình Hệ thống tỷ lệ đổi điểm (SystemConfigs)
CREATE TABLE SystemConfigs (
    ConfigKey NVARCHAR(50) PRIMARY KEY,
    ConfigValue NVARCHAR(255) NOT NULL,
    Description NVARCHAR(255) NULL
);

-- 25. Bảng Đăng ký Nhận thông báo (PushSubscriptions)
CREATE TABLE PushSubscriptions (
    SubscriptionId INT PRIMARY KEY IDENTITY(1,1),
    Phone NVARCHAR(15) NOT NULL,
    Endpoint NVARCHAR(MAX) NOT NULL,
    P256DH NVARCHAR(MAX) NOT NULL,
    Auth NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

GO
PRINT 'TẠO CƠ SỞ DỮ LIỆU QR DRINK ORDER THÀNH CÔNG!'