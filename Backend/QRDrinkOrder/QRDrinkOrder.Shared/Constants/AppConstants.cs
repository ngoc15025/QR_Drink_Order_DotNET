namespace QRDrinkOrder.Shared.Constants;

public static class AppRoles
{
    public const byte AdminId = 1;
    public const byte ManagerId = 2;
    public const byte EmployeeId = 3;

    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Employee = "Employee";

    public static string GetRoleName(byte roleId)
    {
        return roleId switch
        {
            AdminId => Admin,
            ManagerId => Manager,
            EmployeeId => Employee,
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
    public const string CouponAlreadyUsed = "Số điện thoại này đã sử dụng mã giảm giá này rồi.";
    public const string MinOrderNotMet = "Đơn hàng chưa đạt giá trị tối thiểu để áp dụng mã giảm giá.";
    public const string OrderCannotCancel = "Đơn hàng chỉ có thể hủy khi đang ở trạng thái Chờ thanh toán.";
    public const string OrderNotFound = "Không tìm thấy thông tin đơn hàng.";
    public const string AccountDisabled = "Tài khoản của bạn đã bị vô hiệu hóa.";
}
