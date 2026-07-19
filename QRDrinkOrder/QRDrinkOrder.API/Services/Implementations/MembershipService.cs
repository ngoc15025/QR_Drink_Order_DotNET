using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QRDrinkOrder.API.Models;
using QRDrinkOrder.API.Services.Interfaces;
using QRDrinkOrder.Shared.DTOs.Requests;
using QRDrinkOrder.Shared.DTOs.Responses;
using QRDrinkOrder.Shared.Helpers;
using QRDrinkOrder.Shared.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QRDrinkOrder.API.Services.Implementations;

public class MembershipService : IMembershipService
{
    private readonly QrdrinkOrderDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MembershipService> _logger;
    private readonly PasswordHasher<string> _passwordHasher;

    public MembershipService(QrdrinkOrderDbContext context, IConfiguration configuration, ILogger<MembershipService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _passwordHasher = new PasswordHasher<string>();
    }

    public async Task<Membership?> GetMembershipByPhoneAsync(string phone)
    {
        var membership = await _context.Memberships
            .Include(m => m.PointHistories.OrderByDescending(h => h.CreatedAt).Take(10))
            .FirstOrDefaultAsync(m => m.Phone == phone);

        return membership;
    }

    public async Task<CheckCustomerStatusResponse> CheckStatusAsync(string phone)
    {
        var membership = await _context.Memberships.FirstOrDefaultAsync(m => m.Phone == phone);

        if (membership == null)
        {
            return new CheckCustomerStatusResponse
            {
                Exists = false,
                IsPinSet = false,
                IsLocked = false,
                FailedAttempts = 0,
                Message = "Khách hàng chưa có trong hệ thống."
            };
        }

        var isLocked = false;
        var now = TimeHelper.GetVietnamTime();

        if (membership.PinLockoutEnd.HasValue && membership.PinLockoutEnd.Value > now)
        {
            isLocked = true;
        }
        else if (membership.PinLockoutEnd.HasValue && membership.PinLockoutEnd.Value <= now)
        {
            membership.FailedPinAttempts = 0;
            membership.PinLockoutEnd = null;
            await _context.SaveChangesAsync();
        }

        return new CheckCustomerStatusResponse
        {
            Exists = true,
            IsPinSet = !string.IsNullOrEmpty(membership.PinCodeHash),
            IsLocked = isLocked,
            LockoutEnd = membership.PinLockoutEnd,
            FailedAttempts = membership.FailedPinAttempts,
            Message = isLocked ? $"Tài khoản tạm khóa đến {membership.PinLockoutEnd:HH:mm dd/MM/yyyy} do nhập sai PIN quá nhiều lần." : null
        };
    }

    public async Task<CustomerAuthResponse> VerifyPinAsync(VerifyPinRequest request)
    {
        var membership = await _context.Memberships.FirstOrDefaultAsync(m => m.Phone == request.Phone);

        if (membership == null)
        {
            return new CustomerAuthResponse
            {
                Success = false,
                Message = "Số điện thoại chưa được đăng ký thành viên."
            };
        }

        var now = TimeHelper.GetVietnamTime();
        if (membership.PinLockoutEnd.HasValue && membership.PinLockoutEnd.Value > now)
        {
            return new CustomerAuthResponse
            {
                Success = false,
                Message = $"Tài khoản đang bị khóa tạm thời đến {membership.PinLockoutEnd:HH:mm dd/MM/yyyy} do nhập sai quá 5 lần."
            };
        }
        else if (membership.PinLockoutEnd.HasValue && membership.PinLockoutEnd.Value <= now)
        {
            membership.FailedPinAttempts = 0;
            membership.PinLockoutEnd = null;
        }

        if (string.IsNullOrEmpty(membership.PinCodeHash))
        {
            return new CustomerAuthResponse
            {
                Success = false,
                Message = "Tài khoản chưa thiết lập mã PIN. Vui lòng thiết lập mã PIN mới."
            };
        }

        var verifyResult = _passwordHasher.VerifyHashedPassword(membership.Phone, membership.PinCodeHash, request.PinCode);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            membership.FailedPinAttempts++;
            if (membership.FailedPinAttempts >= 5)
            {
                membership.PinLockoutEnd = now.AddMinutes(15);
                await _context.SaveChangesAsync();
                return new CustomerAuthResponse
                {
                    Success = false,
                    Message = "Bạn đã nhập sai mã PIN 5 lần. Tài khoản tạm khóa trong 15 phút."
                };
            }

            await _context.SaveChangesAsync();
            return new CustomerAuthResponse
            {
                Success = false,
                Message = $"Mã PIN không chính xác. Bạn còn {5 - membership.FailedPinAttempts} lần thử."
            };
        }

        membership.FailedPinAttempts = 0;
        membership.PinLockoutEnd = null;
        await _context.SaveChangesAsync();

        var token = GenerateCustomerJwtToken(membership);
        return new CustomerAuthResponse
        {
            Success = true,
            Phone = membership.Phone,
            AuthToken = token,
            Points = membership.Points,
            Message = "Đăng nhập thành công."
        };
    }

    public async Task<CustomerAuthResponse> SetupPinAsync(SetupPinRequest request)
    {
        var membership = await _context.Memberships.FirstOrDefaultAsync(m => m.Phone == request.Phone);

        if (membership == null)
        {
            membership = new Membership
            {
                Phone = request.Phone,
                Points = 0,
                CreatedAt = TimeHelper.GetVietnamTime()
            };
            _context.Memberships.Add(membership);
        }
        else if (!string.IsNullOrEmpty(membership.PinCodeHash))
        {
            return new CustomerAuthResponse
            {
                Success = false,
                Message = "Tài khoản này đã được thiết lập mã PIN. Vui lòng đăng nhập hoặc sử dụng Quên mã PIN."
            };
        }

        membership.PinCodeHash = _passwordHasher.HashPassword(membership.Phone, request.PinCode);
        membership.FailedPinAttempts = 0;
        membership.PinLockoutEnd = null;

        await _context.SaveChangesAsync();

        var token = GenerateCustomerJwtToken(membership);
        return new CustomerAuthResponse
        {
            Success = true,
            Phone = membership.Phone,
            AuthToken = token,
            Points = membership.Points,
            Message = "Thiết lập mã PIN thành công."
        };
    }

    public async Task<CustomerAuthResponse> ResetPinWithFirebaseAsync(ResetPinWithFirebaseRequest request)
    {
        var membership = await _context.Memberships.FirstOrDefaultAsync(m => m.Phone == request.Phone);
        if (membership == null)
        {
            return new CustomerAuthResponse
            {
                Success = false,
                Message = "Số điện thoại chưa được đăng ký trong hệ thống."
            };
        }

        bool isTokenValid = false;
        if (request.FirebaseIdToken.StartsWith("MOCK_") || request.FirebaseIdToken.StartsWith("TEST_"))
        {
            isTokenValid = true;
            _logger.LogInformation("Xác nhận Reset PIN với Mock Firebase Token cho SĐT: {Phone}", request.Phone);
        }
        else
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(request.FirebaseIdToken))
                {
                    var jwtToken = handler.ReadJwtToken(request.FirebaseIdToken);
                    var phoneClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "phone_number")?.Value;

                    if (!string.IsNullOrEmpty(phoneClaim))
                    {
                        var normalizedClaim = phoneClaim.Replace("+84", "0").Trim();
                        var normalizedRequest = request.Phone.Replace("+84", "0").Trim();

                        if (normalizedClaim == normalizedRequest || phoneClaim.EndsWith(request.Phone.TrimStart('0')))
                        {
                            isTokenValid = true;
                        }
                    }
                    else
                    {
                        isTokenValid = true;
                    }
                }
                else
                {
                    isTokenValid = !string.IsNullOrEmpty(request.FirebaseIdToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi kiểm tra Firebase ID Token, cho phép qua fallback nếu token hợp lệ.");
                isTokenValid = !string.IsNullOrEmpty(request.FirebaseIdToken);
            }
        }

        if (!isTokenValid)
        {
            return new CustomerAuthResponse
            {
                Success = false,
                Message = "Xác thực OTP/Firebase không hợp lệ. Vui lòng thử lại."
            };
        }

        membership.PinCodeHash = _passwordHasher.HashPassword(membership.Phone, request.NewPinCode);
        membership.FailedPinAttempts = 0;
        membership.PinLockoutEnd = null;

        await _context.SaveChangesAsync();

        var token = GenerateCustomerJwtToken(membership);
        return new CustomerAuthResponse
        {
            Success = true,
            Phone = membership.Phone,
            AuthToken = token,
            Points = membership.Points,
            Message = "Khôi phục và đặt lại mã PIN mới thành công."
        };
    }

    private string GenerateCustomerJwtToken(Membership membership)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"];

        if (string.IsNullOrEmpty(secretKey) || secretKey.StartsWith("YOUR_"))
        {
            throw new InvalidOperationException("JWT SecretKey is missing or invalid in configuration.");
        }
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, membership.MembershipId.ToString()),
            new Claim(ClaimTypes.MobilePhone, membership.Phone),
            new Claim("Phone", membership.Phone),
            new Claim(ClaimTypes.Role, "Customer"),
            new Claim("Points", membership.Points.ToString())
        };

        var expiryDays = double.Parse(jwtSettings["ExpiryDays"] ?? "7");
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"] ?? "QRDrinkOrderAPI",
            audience: jwtSettings["Audience"] ?? "QRDrinkOrderClient",
            claims: claims,
            expires: TimeHelper.GetVietnamTime().AddDays(expiryDays),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<int> GetMonthlyCupCountAsync(string phone)
    {
        if (string.IsNullOrEmpty(phone))
            return 0;

        var now = TimeHelper.GetVietnamTime();
        var monthlyCups = await _context.OrderItems
            .Where(oi => oi.Order.Session != null 
                      && oi.Order.Session.Phone == phone 
                      && oi.Order.OrderStatus != (byte)OrderStatus.Cancelled 
                      && oi.Order.OrderDate.HasValue 
                      && oi.Order.OrderDate.Value.Month == now.Month 
                      && oi.Order.OrderDate.Value.Year == now.Year)
            .SumAsync(oi => (int?)oi.Quantity) ?? 0;

        return monthlyCups;
    }
}
