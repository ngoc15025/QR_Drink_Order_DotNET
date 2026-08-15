namespace QRDrinkOrder.Shared.Constants;

public static class AppRoles
{
    public const byte AdminId = 1;
    public const byte ManagerId = 2;
    public const byte BaristaId = 3;
    public const byte WaiterId = 4;

    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Barista = "Barista";
    public const string Waiter = "Waiter"; 

    public static string GetRoleName(byte roleId)
    {
        return roleId switch
        {
            AdminId => Admin,
            ManagerId => Manager,
            BaristaId => Barista,
            WaiterId => Waiter,
            _ => "Guest"
        };
    }
}

public static class AppLanguages
{
    public const string Vietnamese = "vi";
    public const string English = "en";
    public const string Default = Vietnamese;
}

public static class ErrorMessages
{
    public const string Unauthorized = "Bạn không có quyền thực hiện chức năng này.";
    public const string SessionExpired = "Phiên làm việc đã hết hạn.";
    public const string InvalidCoupon = "Mã giảm giá không hợp lệ hoặc đã hết hạn.";
    public const string CouponLimitReached = "Mã giảm giá đã đạt giới hạn sử dụng.";
    public const string CouponAlreadyUsed = "Một số điện thoại chỉ được áp dụng mã một lần.";
    public const string MinOrderNotMet = "Đơn hàng chưa đạt giá trị tối thiểu để áp dụng mã giảm giá.";
    public const string OrderCannotCancel = "Đơn hàng chỉ có thể hủy khi đang ở trạng thái Chờ thanh toán.";
    public const string OrderNotFound = "Không tìm thấy thông tin đơn hàng.";
    public const string AccountDisabled = "Tài khoản của bạn đã bị vô hiệu hóa.";
}
