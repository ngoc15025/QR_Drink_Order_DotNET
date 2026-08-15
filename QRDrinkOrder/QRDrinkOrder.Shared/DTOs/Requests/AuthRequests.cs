using QRDrinkOrder.Shared.Attributes;
using System.ComponentModel.DataAnnotations;

namespace QRDrinkOrder.Shared.DTOs.Requests;

public class LoginRequest
{
    [RequiredString(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ.")]
    public string Email { get; set; } = string.Empty;

    [RequiredString(ErrorMessage = "Mật khẩu là bắt buộc.")]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [RequiredString(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ.")]
    public string Email { get; set; } = string.Empty;

    [RequiredString(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
    public string Password { get; set; } = string.Empty;

    [RequiredString(ErrorMessage = "Họ tên là bắt buộc.")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string? Phone { get; set; }

    [RequiredString(ErrorMessage = "Vai trò là bắt buộc.")]
    public byte RoleId { get; set; }
}

public class ChangePasswordRequest
{
    [RequiredString(ErrorMessage = "Mật khẩu cũ là bắt buộc.")]
    public string OldPassword { get; set; } = string.Empty;

    [RequiredString(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
    [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
    public string NewPassword { get; set; } = string.Empty;
}

public class AccountStatusRequest
{
    public bool IsActive { get; set; }
}

public class UpdateAccountRequest
{
    [RequiredString(ErrorMessage = "Họ tên là bắt buộc.")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ.")]
    public string? Email { get; set; }

    public byte RoleId { get; set; }
}

public class AdminResetPasswordRequest
{
    [RequiredString(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
    [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
    public string NewPassword { get; set; } = string.Empty;
}

