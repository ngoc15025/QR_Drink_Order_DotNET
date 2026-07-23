USE QRDrinkOrderDB;
GO

-- Xóa dữ liệu cũ nếu muốn (Tuỳ chọn, ở đây mình thêm mới nên có thể không cần xoá hoặc xoá hết để làm sạch)
DELETE FROM PromotionTranslations;
DELETE FROM Promotions;
DELETE FROM Coupons WHERE CouponCode IN ('SUMMER20', 'BOGOFE', 'NEW15K');

-- Coupon 1: Mùa Hè Sôi Động (SUMMER20) - Giảm 20%, tối đa 50k, đơn từ 100k
INSERT INTO Coupons (CouponCode, DiscountType, DiscountValue, MinOrderValue, MaxDiscountAmount, UsageLimit, UsedCount, StartDate, EndDate, IsActive)
VALUES ('SUMMER20', 0, 20.00, 100000.00, 50000.00, 1000, 0, GETDATE(), DATEADD(day, 30, GETDATE()), 1);
DECLARE @Coupon1Id INT = SCOPE_IDENTITY();

INSERT INTO Promotions (ImageUrl, CouponId, IsActive, CreatedAt)
VALUES ('/images/promotions/promo_summer.png', @Coupon1Id, 1, GETDATE());
DECLARE @Promo1Id INT = SCOPE_IDENTITY();

INSERT INTO PromotionTranslations (PromotionId, LanguageCode, Title, Content)
VALUES (@Promo1Id, 'vi', N'Mùa Hè Sôi Động', N'Giảm ngay 20% (tối đa 50K) cho đơn hàng từ 100K. Thưởng thức đồ uống mát lạnh thổi bay cái nóng mùa hè! Nhập mã SUMMER20 lúc thanh toán.'),
       (@Promo1Id, 'en', N'Summer Vibes', N'Get 20% off (up to 50K) for orders from 100K. Enjoy our refreshing cold drinks to beat the summer heat! Apply code SUMMER20 at checkout.');


-- Coupon 2: Thứ Ba Hạnh Phúc (BOGOFE) - Giảm 35k
INSERT INTO Coupons (CouponCode, DiscountType, DiscountValue, MinOrderValue, MaxDiscountAmount, UsageLimit, UsedCount, StartDate, EndDate, IsActive)
VALUES ('BOGOFE', 1, 35000.00, 50000.00, 35000.00, 500, 0, GETDATE(), DATEADD(day, 60, GETDATE()), 1);
DECLARE @Coupon2Id INT = SCOPE_IDENTITY();

INSERT INTO Promotions (ImageUrl, CouponId, IsActive, CreatedAt)
VALUES ('/images/promotions/promo_bogo.png', @Coupon2Id, 1, GETDATE());
DECLARE @Promo2Id INT = SCOPE_IDENTITY();

INSERT INTO PromotionTranslations (PromotionId, LanguageCode, Title, Content)
VALUES (@Promo2Id, 'vi', N'Thứ Ba Hạnh Phúc', N'Ngày thứ Ba vui vẻ, giảm ngay 35K cho đơn hàng từ 50K. Rủ ngay bạn bè cùng đến thưởng thức cà phê ngon tuyệt! Mã: BOGOFE.'),
       (@Promo2Id, 'en', N'Happy Tuesday', N'Happy Tuesday! Get 35K off for orders from 50K. Grab your friends and enjoy our delicious coffee! Code: BOGOFE.');


-- Coupon 3: Khách Hàng Mới (NEW15K) - Giảm 15k
INSERT INTO Coupons (CouponCode, DiscountType, DiscountValue, MinOrderValue, MaxDiscountAmount, UsageLimit, UsedCount, StartDate, EndDate, IsActive)
VALUES ('NEW15K', 1, 15000.00, 0, 15000.00, 2000, 0, GETDATE(), DATEADD(year, 1, GETDATE()), 1);
DECLARE @Coupon3Id INT = SCOPE_IDENTITY();

INSERT INTO Promotions (ImageUrl, CouponId, IsActive, CreatedAt)
VALUES ('/images/promotions/promo_welcome.png', @Coupon3Id, 1, GETDATE());
DECLARE @Promo3Id INT = SCOPE_IDENTITY();

INSERT INTO PromotionTranslations (PromotionId, LanguageCode, Title, Content)
VALUES (@Promo3Id, 'vi', N'Chào Bạn Mới', N'Chào mừng bạn đến với Ngoc UwU Coffee! Tặng bạn mã giảm 15K không giới hạn giá trị đơn hàng. Nhập mã NEW15K ngay.'),
       (@Promo3Id, 'en', N'Welcome Friend', N'Welcome to Ngoc UwU Coffee! Here is a 15K discount code for your first order, no minimum required. Apply NEW15K now.');

GO
