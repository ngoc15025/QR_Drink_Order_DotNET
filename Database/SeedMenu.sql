USE QRDrinkOrderDB;
GO

-- Xóa các liên kết trước để tránh lỗi khóa ngoại (Foreign Key Constraints)
DELETE FROM ReviewImages;
DELETE FROM Reviews;
DELETE FROM Payments;
DELETE FROM CouponUsages;
DELETE FROM StaffBenefits;
DELETE FROM OrderItemToppings;
DELETE FROM OrderItems;
DELETE FROM Orders;
DELETE FROM CustomerSessions;

-- Xóa dữ liệu cũ
DELETE FROM DrinkTranslations;
DELETE FROM Drinks;
DELETE FROM CategoryTranslations;
DELETE FROM Categories;
DELETE FROM Sizes;
DELETE FROM Toppings;
GO

-- Reset IDENTITY
DBCC CHECKIDENT ('Categories', RESEED, 0);
DBCC CHECKIDENT ('CategoryTranslations', RESEED, 0);
DBCC CHECKIDENT ('Drinks', RESEED, 0);
DBCC CHECKIDENT ('DrinkTranslations', RESEED, 0);
DBCC CHECKIDENT ('Orders', RESEED, 0);
DBCC CHECKIDENT ('OrderItems', RESEED, 0);
DBCC CHECKIDENT ('Sizes', RESEED, 0);
DBCC CHECKIDENT ('Toppings', RESEED, 0);
GO

-- =========================================================
-- 1. TẠO DỮ LIỆU DANH MỤC (CATEGORIES)
-- =========================================================
SET IDENTITY_INSERT Categories ON;
INSERT INTO Categories (CategoryID, DisplayOrder, IsActive, CreatedAt)
VALUES 
(1, 1, 1, GETDATE()), -- Cà phê
(2, 2, 1, GETDATE()), -- Trà & Trà Sữa
(3, 3, 1, GETDATE()), -- Sinh tố & Nước ép
(4, 4, 1, GETDATE()); -- Bánh ngọt ăn kèm
SET IDENTITY_INSERT Categories OFF;

-- =========================================================
-- 2. TẠO DỮ LIỆU ĐA NGÔN NGỮ CHO DANH MỤC (CATEGORY TRANSLATIONS)
-- =========================================================
SET IDENTITY_INSERT CategoryTranslations ON;
INSERT INTO CategoryTranslations (CategoryTranslationID, CategoryID, LanguageCode, CategoryName)
VALUES 
(1, 1, 'vi', N'Cà phê / Coffee'),
(2, 1, 'en', N'Coffee'),
(3, 2, 'vi', N'Trà & Trà Sữa'),
(4, 2, 'en', N'Tea & Milk Tea'),
(5, 3, 'vi', N'Sinh Tố & Nước Ép'),
(6, 3, 'en', N'Smoothies & Juices'),
(7, 4, 'vi', N'Bánh Ngọt'),
(8, 4, 'en', N'Pastries & Cakes');
SET IDENTITY_INSERT CategoryTranslations OFF;

-- =========================================================
-- SIZES AND TOPPINGS SEEDING
-- =========================================================
SET IDENTITY_INSERT Sizes ON;
INSERT INTO Sizes (SizeID, Name, PriceOffset)
VALUES 
(1, N'Size S', 0),
(2, N'Size M', 5000),
(3, N'Size L', 10000);
SET IDENTITY_INSERT Sizes OFF;

SET IDENTITY_INSERT Toppings ON;
INSERT INTO Toppings (ToppingID, Name, Price)
VALUES 
(1, N'Trân châu đen', 5000),
(2, N'Trân châu trắng', 6000),
(3, N'Thạch nha đam', 5000),
(4, N'Kem phô mai', 10000),
(5, N'Đào miếng', 10000);
SET IDENTITY_INSERT Toppings OFF;

-- =========================================================
-- 3. TẠO DỮ LIỆU MÓN NƯỚC (DRINKS)
-- TemperatureType: 0=Nóng, 1=Lạnh, 2=Tùy chọn, 3=Bánh/Khác
-- =========================================================
SET IDENTITY_INSERT Drinks ON;
INSERT INTO Drinks (DrinkID, CategoryID, ImageUrl, BasePrice, TemperatureType, IsActive, CreatedAt)
VALUES 
-- Nhóm 1: Cà Phê (7 món)
(1, 1, 'https://cdn.tgdd.vn/Files/2021/08/12/1374706/cach-pha-ca-phe-sua-da-ngon-chuan-vi-quan-ca-phe-202108121655079148.jpg', 29000, 1, 1, GETDATE()),
(2, 1, 'https://cdn.tgdd.vn/Files/2021/08/18/1376043/huong-dan-cach-lam-bac-xiu-da-ngon-kho-cuong-202201201530348736.jpeg', 35000, 2, 1, GETDATE()),
(3, 1, 'https://capherangxay.vn/wp-content/uploads/2022/10/cafe-americano-la-gi.jpg', 39000, 1, 1, GETDATE()),
(4, 1, 'https://cdn.tgdd.vn/Files/2019/08/02/1183884/cach-pha-cafe-latte-don-gian-va-nhanh-chong-tai-nha-201908022014494164.jpg', 45000, 2, 1, GETDATE()),
(5, 1, 'https://cdn.tgdd.vn/Files/2019/08/06/1184318/cach-pha-cafe-cappuccino-sieu-ngon-sieu-don-gian-tai-nha-201908061309536098.jpg', 45000, 0, 1, GETDATE()),
(6, 1, 'https://cdn.tgdd.vn/Files/2020/03/09/1241193/mach-ban-cach-lam-caramel-macchiato-tai-nha-ngon-n-1.jpg', 55000, 2, 1, GETDATE()),
(7, 1, 'https://cdn.tgdd.vn/Files/2020/08/25/1283626/cold-brew-la-gi-co-khac-biet-gi-voi-ca-phe-pha-phin-202008251025239088.jpg', 49000, 1, 1, GETDATE()),

-- Nhóm 2: Trà & Trà Sữa (6 món)
(8, 2, 'https://cdn.tgdd.vn/Files/2020/04/04/1246736/cach-pha-tra-sua-tran-chau-duong-den-tai-nha-ngon-nhu-ngoai-quan-202112311400494493.jpg', 45000, 1, 1, GETDATE()),
(9, 2, 'https://cdn.tgdd.vn/Files/2021/08/25/1377855/cach-lam-tra-dao-cam-sa-giai-nhiet-mua-he-202201081335029410.jpeg', 49000, 1, 1, GETDATE()),
(10, 2, 'https://cdn.tgdd.vn/Files/2021/08/11/1374520/cach-lam-tra-vai-thom-ngon-mat-lanh-giai-khat-mua-he-202108111629088514.jpg', 49000, 1, 1, GETDATE()),
(11, 2, 'https://cdn.tgdd.vn/2021/03/CookRecipe/Avatar/tra-o-long-hat-sen-thuong-dinh-thumbnail.jpg', 55000, 1, 1, GETDATE()),
(12, 2, 'https://cdn.tgdd.vn/Files/2021/03/17/1336186/mach-ban-2-cach-lam-tra-sua-matcha-tran-chau-duong-den-ngon-xuat-sac-202103171439223835.jpg', 45000, 1, 1, GETDATE()),
(13, 2, 'https://cdn.tgdd.vn/Files/2019/11/27/1222474/cach-lam-tra-xoai-macchiato-thanh-mat-thom-beo-giai-nhiet-he-cuc-tot-201911270940391295.jpg', 55000, 1, 1, GETDATE()),

-- Nhóm 3: Sinh Tố & Nước Ép (6 món)
(14, 3, 'https://cdn.tgdd.vn/2020/07/CookRecipe/Avatar/nuoc-ep-dua-hau-thumbnail-1.jpg', 35000, 1, 1, GETDATE()),
(15, 3, 'https://cdn.tgdd.vn/Files/2021/08/19/1376378/cach-lam-sinh-to-bo-thom-ngon-beo-ngay-khong-bi-dang-202108191142588496.jpg', 49000, 1, 1, GETDATE()),
(16, 3, 'https://cdn.tgdd.vn/Files/2021/08/17/1375836/2-cach-lam-sinh-to-xoai-nuoc-cot-dua-thom-ngon-don-gian-nhat-202108172152520625.jpeg', 49000, 1, 1, GETDATE()),
(17, 3, 'https://cdn.tgdd.vn/2020/07/CookRecipe/Avatar/nuoc-ep-cam-thumbnail-1.jpg', 39000, 1, 1, GETDATE()),
(18, 3, 'https://cdn.tgdd.vn/2020/07/CookRecipe/Avatar/nuoc-ep-thom-thumbnail-1.jpg', 39000, 1, 1, GETDATE()),
(19, 3, 'https://cdn.tgdd.vn/2021/01/CookRecipe/Avatar/nuoc-ep-can-tay-tao-thumbnail.jpg', 45000, 1, 1, GETDATE()),

-- Nhóm 4: Bánh ngọt (5 món)
(20, 4, 'https://cdn.tgdd.vn/Files/2020/06/18/1263884/cach-lam-banh-tiramisu-khong-can-lo-nuong-202006181045230910.jpg', 39000, 3, 1, GETDATE()),
(21, 4, 'https://cdn.tgdd.vn/2021/01/CookRecipe/Avatar/banh-sung-bo-croissant-thumbnail.jpg', 29000, 3, 1, GETDATE()),
(22, 4, 'https://cdn.tgdd.vn/Files/2021/11/02/1393608/cach-lam-banh-cheesecake-chay-basque-burnt-cheesecake-mem-min-don-gian-202111021415255479.jpg', 45000, 3, 1, GETDATE()),
(23, 4, 'https://cdn.tgdd.vn/2021/04/CookRecipe/Avatar/banh-mousse-tra-xanh-thumbnail.jpg', 42000, 3, 1, GETDATE()),
(24, 4, 'https://cdn.tgdd.vn/Files/2020/12/16/1314059/red-velvet-la-gi-bi-quyet-lam-banh-red-velvet-don-gian-nhat-202012162021116244.jpg', 45000, 3, 1, GETDATE());
SET IDENTITY_INSERT Drinks OFF;

-- =========================================================
-- 4. TẠO DỮ LIỆU ĐA NGÔN NGỮ CHO MÓN NƯỚC (DRINK TRANSLATIONS)
-- =========================================================
SET IDENTITY_INSERT DrinkTranslations ON;
INSERT INTO DrinkTranslations (TranslationID, DrinkID, LanguageCode, DrinkName, Description)
VALUES 
-- Cà phê
(1, 1, 'vi', N'Cà Phê Sữa Đá', N'Cà phê pha phin truyền thống kết hợp cùng sữa đặc béo ngậy.'),
(2, 1, 'en', N'Iced Milk Coffee', N'Traditional Vietnamese drip coffee combined with rich condensed milk.'),
(3, 2, 'vi', N'Bạc Xỉu', N'Sự hòa quyện hoàn hảo giữa nhiều sữa và một chút cà phê để tạo điểm nhấn.'),
(4, 2, 'en', N'White Coffee (Bac Xiu)', N'A perfect blend of abundant milk with a hint of coffee for flavor.'),
(5, 3, 'vi', N'Americano Đá', N'Espresso pha loãng với nước tinh khiết, mang lại hương vị cà phê nguyên bản.'),
(6, 3, 'en', N'Iced Americano', N'Espresso diluted with pure water, preserving the original coffee flavor.'),
(7, 4, 'vi', N'Latte Đá', N'Espresso hòa quyện với sữa tươi thanh mát.'),
(8, 4, 'en', N'Iced Latte', N'Espresso combined with refreshing fresh milk.'),
(9, 5, 'vi', N'Cappuccino Nóng', N'Cà phê Ý với lớp bọt sữa nóng bồng bềnh, rắc thêm bột cacao.'),
(10, 5, 'en', N'Hot Cappuccino', N'Italian coffee with fluffy hot milk foam, sprinkled with cocoa powder.'),
(11, 6, 'vi', N'Caramel Macchiato', N'Vị đắng espresso kết hợp với sốt caramel ngọt ngào và sữa tươi.'),
(12, 6, 'en', N'Caramel Macchiato', N'Espresso bitterness combined with sweet caramel sauce and fresh milk.'),
(13, 7, 'vi', N'Cà Phê Ủ Lạnh (Cold Brew)', N'Cà phê được ủ lạnh 24h, mang lại hương vị êm ái, ít đắng, thoảng vị trái cây.'),
(14, 7, 'en', N'Cold Brew Coffee', N'Coffee cold-brewed for 24h, offering a smooth, less bitter taste with fruity notes.'),

-- Trà & Trà Sữa
(15, 8, 'vi', N'Trà Sữa Trân Châu Đường Đen', N'Hồng trà đậm vị kết hợp trân châu nấu đường đen dẻo dai.'),
(16, 8, 'en', N'Brown Sugar Boba Milk Tea', N'Strong black tea paired with chewy brown sugar tapioca pearls.'),
(17, 9, 'vi', N'Trà Đào Cam Sả', N'Vị trà thanh mát, thơm lừng hương sả và cam tươi, kèm đào ngâm giòn ngọt.'),
(18, 9, 'en', N'Peach Orange Lemongrass Tea', N'Refreshing tea infused with lemongrass, fresh orange, and crunchy peach slices.'),
(19, 10, 'vi', N'Trà Vải Nhiệt Đới', N'Trà đen kết hợp cùng vải thiều thơm ngọt và nha đam giòn sần sật.'),
(20, 10, 'en', N'Tropical Lychee Tea', N'Black tea combined with sweet lychee and crunchy aloe vera.'),
(21, 11, 'vi', N'Trà Ô Long Kem Phô Mai', N'Trà Ô long nướng sương mù phủ lớp kem phô mai mặn mặn béo ngậy.'),
(22, 11, 'en', N'Oolong Macchiato', N'Roasted Oolong tea topped with a savory and rich cream cheese layer.'),
(23, 12, 'vi', N'Trà Sữa Matcha', N'Bột trà xanh nguyên chất Nhật Bản hòa quyện cùng sữa tươi béo ngậy.'),
(24, 12, 'en', N'Matcha Milk Tea', N'Pure Japanese matcha powder blended with rich fresh milk.'),
(25, 13, 'vi', N'Trà Xoài Macchiato', N'Trà nhài thanh mát kết hợp cùng mứt xoài tươi và lớp kem phô mai béo.'),
(26, 13, 'en', N'Mango Macchiato Tea', N'Refreshing jasmine tea combined with fresh mango jam and rich cream cheese.'),

-- Sinh tố & Nước ép
(27, 14, 'vi', N'Nước Ép Dưa Hấu', N'Ép chậm từ dưa hấu tươi mát, giải nhiệt ngày hè cực tốt.'),
(28, 14, 'en', N'Watermelon Juice', N'Cold-pressed from fresh watermelon, an excellent summer cooler.'),
(29, 15, 'vi', N'Sinh Tố Bơ Đắk Lắk', N'Bơ sáp béo ngậy xay cùng sữa đặc và đá xay nhuyễn mịn.'),
(30, 15, 'en', N'Avocado Smoothie', N'Creamy avocado blended perfectly with condensed milk and crushed ice.'),
(31, 16, 'vi', N'Sinh Tố Xoài Dừa', N'Sự kết hợp bùng nổ giữa xoài chín và nước cốt dưa béo ngậy.'),
(32, 16, 'en', N'Mango Coconut Smoothie', N'An explosive combination of ripe mango and rich coconut milk.'),
(33, 17, 'vi', N'Nước Ép Cam', N'Nước ép cam tươi nguyên chất, giàu vitamin C.'),
(34, 17, 'en', N'Fresh Orange Juice', N'Pure fresh orange juice, rich in Vitamin C.'),
(35, 18, 'vi', N'Nước Ép Thơm (Dứa)', N'Nước ép dứa thơm lừng, chua ngọt tự nhiên, kích thích tiêu hóa.'),
(36, 18, 'en', N'Pineapple Juice', N'Fragrant pineapple juice, naturally sweet and sour, aids digestion.'),
(37, 19, 'vi', N'Nước Ép Táo Cần Tây', N'Đồ uống detox hoàn hảo, giúp thanh lọc cơ thể và giữ dáng.'),
(38, 19, 'en', N'Apple Celery Juice', N'Perfect detox drink, helping to purify the body and keep in shape.'),

-- Bánh ngọt
(39, 20, 'vi', N'Bánh Tiramisu Ý', N'Bánh xốp mềm thấm vị cà phê, phủ lớp kem mascarpone và bột cacao.'),
(40, 20, 'en', N'Italian Tiramisu', N'Soft cake soaked in coffee, topped with mascarpone cream and cocoa powder.'),
(41, 21, 'vi', N'Bánh Croissant Nướng', N'Bánh sừng trâu nướng bơ Pháp thơm lừng, ngàn lớp giòn rụm.'),
(42, 21, 'en', N'Butter Croissant', N'Freshly baked French butter croissant with thousands of crispy layers.'),
(43, 22, 'vi', N'Bánh Phô Mai Nướng (Basque)', N'Bánh phô mai nướng với bề mặt cháy xém đặc trưng và lớp nhân tan chảy.'),
(44, 22, 'en', N'Basque Burnt Cheesecake', N'Baked cheesecake with a signature burnt top and a melting center.'),
(45, 23, 'vi', N'Mousse Trà Xanh', N'Bánh mousse mềm mịn với vị đắng nhẹ và thơm lừng từ bột trà xanh Nhật.'),
(46, 23, 'en', N'Matcha Mousse', N'Smooth mousse cake with a slight bitterness and aroma from Japanese matcha.'),
(47, 24, 'vi', N'Bánh Red Velvet', N'Bánh nhung đỏ mềm xốp kết hợp cùng lớp kem phô mai chua ngọt hài hòa.'),
(48, 24, 'en', N'Red Velvet Cake', N'Soft red velvet cake combined with a harmoniously sweet and sour cream cheese layer.');
SET IDENTITY_INSERT DrinkTranslations OFF;

-- =========================================================
-- 5. MÔ PHỎNG DỮ LIỆU ĐƠN HÀNG (GIẢ LẬP LƯỢT ĐẶT CHO AI)
-- =========================================================
-- Chúng ta tạo sẵn một session
DECLARE @MockSession UNIQUEIDENTIFIER = NEWID();
INSERT INTO CustomerSessions (SessionID, Phone, DeviceInfo, CreatedAt)
VALUES (@MockSession, '0987654321', 'Mock AI Seed Data', DATEADD(DAY, -30, GETDATE()));

-- Biến vòng lặp tạo đơn hàng
DECLARE @Counter INT = 1;
DECLARE @TotalOrdersToGenerate INT = 200;

WHILE @Counter <= @TotalOrdersToGenerate
BEGIN
    -- Tính toán ngày ngẫu nhiên trong 30 ngày qua
    DECLARE @RandomDate DATETIME2 = DATEADD(DAY, - (ABS(CHECKSUM(NEWID()) % 30)), GETDATE());
    
    -- Tạo đơn hàng
    INSERT INTO Orders (SessionID, TotalAmount, DiscountAmount, OrderStatus, OrderDate)
    VALUES (@MockSession, 0, 0, 2, @RandomDate); -- Status 2: Hoàn thành
    
    DECLARE @NewOrderID INT = SCOPE_IDENTITY();
    DECLARE @OrderTotal DECIMAL(18,2) = 0;
    
    -- Thêm 1-3 món ngẫu nhiên cho đơn hàng này
    DECLARE @ItemCount INT = (ABS(CHECKSUM(NEWID()) % 3) + 1);
    DECLARE @ItemCounter INT = 1;
    
    WHILE @ItemCounter <= @ItemCount
    BEGIN
        -- Phân bổ tỷ lệ (AI Best seller bias)
        -- Chọn ngẫu nhiên 1 DrinkID. Để tạo best seller, ta thiên vị ID = 1, 8, 15
        DECLARE @RandomDrinkID INT;
        DECLARE @Chance INT = ABS(CHECKSUM(NEWID()) % 100);
        
        IF @Chance < 20 SET @RandomDrinkID = 1; -- Cà phê sữa đá (20% xác suất)
        ELSE IF @Chance < 40 SET @RandomDrinkID = 8; -- Trà sữa TCĐĐ (20% xác suất)
        ELSE IF @Chance < 55 SET @RandomDrinkID = 15; -- Sinh tố bơ (15% xác suất)
        ELSE SET @RandomDrinkID = (ABS(CHECKSUM(NEWID()) % 24) + 1); -- Các món khác
        
        DECLARE @DrinkPrice DECIMAL(18,2);
        SELECT @DrinkPrice = BasePrice FROM Drinks WHERE DrinkID = @RandomDrinkID;
        
        DECLARE @Qty INT = (ABS(CHECKSUM(NEWID()) % 2) + 1); -- 1-2 ly
        
        INSERT INTO OrderItems (OrderID, DrinkID, Quantity, SweetnessLevel, IceLevel, UnitPrice)
        VALUES (@NewOrderID, @RandomDrinkID, @Qty, 100, 100, @DrinkPrice);
        
        SET @OrderTotal = @OrderTotal + (@DrinkPrice * @Qty);
        SET @ItemCounter = @ItemCounter + 1;
    END

    -- Cập nhật lại tổng tiền cho đơn hàng
    UPDATE Orders SET TotalAmount = @OrderTotal WHERE OrderID = @NewOrderID;
    
    SET @Counter = @Counter + 1;
END

PRINT N'✅ THÊM DỮ LIỆU MẪU (SEED DATA KÈM MÔ PHỎNG ORDER AI) THÀNH CÔNG!';
