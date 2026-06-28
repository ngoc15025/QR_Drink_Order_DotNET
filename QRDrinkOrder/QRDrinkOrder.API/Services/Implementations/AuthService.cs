using QRDrinkOrder.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QRDrinkOrder.API.Services.Interfaces;
using QRDrinkOrder.Shared.Constants;
using QRDrinkOrder.Shared.DTOs.Requests;
using QRDrinkOrder.Shared.DTOs.Responses;
using QRDrinkOrder.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QRDrinkOrder.API.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly QrdrinkOrderDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<string> _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(QrdrinkOrderDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _passwordHasher = new PasswordHasher<string>();
        _logger = logger;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var account = await _context.Accounts
            .Include(a => a.Role)
            .Include(a => a.Employee)
            .Include(a => a.Manager)
            .FirstOrDefaultAsync(a => a.Email == request.Email && a.IsDeleted == false);

        if (account == null)
            return null;

        if (!account.IsActive == true)
            throw new BusinessException(ErrorMessages.AccountDisabled);

        var verifyResult = _passwordHasher.VerifyHashedPassword(request.Email, account.PasswordHash, request.Password);
        if (verifyResult == PasswordVerificationResult.Failed)
            return null;

        var token = GenerateJwtToken(account);

        int? userId = null;
        if (account.RoleId == AppRoles.EmployeeId)
            userId = account.Employee?.EmployeeId;
        else if (account.RoleId == AppRoles.ManagerId || account.RoleId == AppRoles.AdminId)
            userId = account.Manager?.ManagerId;

        return new AuthResponse
        {
            Token = token,
            Email = account.Email,
            FullName = account.RoleId == AppRoles.EmployeeId ? account.Employee?.FullName ?? "Nhân viên" : account.Manager?.FullName ?? "Quản lý",
            RoleName = account.Role.RoleName,
            RoleId = account.RoleId,
            AccountId = account.AccountId,
            UserId = userId
        };
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        var existingAccount = await _context.Accounts.AnyAsync(a => a.Email == request.Email);
        if (existingAccount)
            throw new BusinessException("Email này đã được sử dụng.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var newAccount = new Account
            {
                Email = request.Email,
                RoleId = request.RoleId,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };

            newAccount.PasswordHash = _passwordHasher.HashPassword(request.Email, request.Password);
            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();

            if (request.RoleId == AppRoles.EmployeeId)
            {
                var employee = new Employee
                {
                    AccountId = newAccount.AccountId,
                    FullName = request.FullName,
                    Phone = request.Phone
                };
                _context.Employees.Add(employee);
            }
            else if (request.RoleId == AppRoles.ManagerId || request.RoleId == AppRoles.AdminId)
            {
                var manager = new Manager
                {
                    AccountId = newAccount.AccountId,
                    FullName = request.FullName,
                    Phone = request.Phone
                };
                _context.Managers.Add(manager);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra trong quá trình đăng ký tài khoản");
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> ChangePasswordAsync(int accountId, ChangePasswordRequest request)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null || account.IsDeleted == true)
            return false;

        var verifyResult = _passwordHasher.VerifyHashedPassword(account.Email, account.PasswordHash, request.OldPassword);
        if (verifyResult == PasswordVerificationResult.Failed)
            throw new BusinessException("Mật khẩu cũ không chính xác.");

        account.PasswordHash = _passwordHasher.HashPassword(account.Email, request.NewPassword);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleAccountStatusAsync(int accountId, bool isActive)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null || account.IsDeleted == true)
            return false;

        account.IsActive = isActive;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<AccountDto>> GetAllAccountsAsync()
    {
        var accounts = await _context.Accounts
            .Include(a => a.Role)
            .Include(a => a.Employee)
            .Include(a => a.Manager)
            .Where(a => a.IsDeleted == false)
            .ToListAsync();

        var list = new List<AccountDto>();
        foreach (var a in accounts)
        {
            list.Add(new AccountDto
            {
                AccountId = a.AccountId,
                Email = a.Email,
                RoleId = a.RoleId,
                RoleName = a.Role.RoleName,
                IsActive = a.IsActive == true,
                CreatedAt = a.CreatedAt ?? DateTime.Now,
                FullName = a.RoleId == AppRoles.EmployeeId ? a.Employee?.FullName ?? "N/A" : a.Manager?.FullName ?? "N/A",
                Phone = a.RoleId == AppRoles.EmployeeId ? a.Employee?.Phone : a.Manager?.Phone
            });
        }
        return list;
    }

    public async Task<AccountDto?> GetAccountByIdAsync(int accountId)
    {
        var a = await _context.Accounts
            .Include(a => a.Role)
            .Include(a => a.Employee)
            .Include(a => a.Manager)
            .FirstOrDefaultAsync(acc => acc.AccountId == accountId && acc.IsDeleted == false);

        if (a == null)
            return null;

        return new AccountDto
        {
            AccountId = a.AccountId,
            Email = a.Email,
            RoleId = a.RoleId,
            RoleName = a.Role.RoleName,
            IsActive = a.IsActive == true,
            CreatedAt = a.CreatedAt ?? DateTime.Now,
            FullName = a.RoleId == AppRoles.EmployeeId ? a.Employee?.FullName ?? "N/A" : a.Manager?.FullName ?? "N/A",
            Phone = a.RoleId == AppRoles.EmployeeId ? a.Employee?.Phone : a.Manager?.Phone
        };
    }

    private string GenerateJwtToken(Account account)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"];

        if (string.IsNullOrEmpty(secretKey) || secretKey.StartsWith("YOUR_"))
        {
            throw new InvalidOperationException("JWT SecretKey is missing or invalid in configuration. Please configure it properly in appsettings.json or environment variables.");
        }
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
            new Claim(ClaimTypes.Email, account.Email),
            new Claim(ClaimTypes.Role, account.Role.RoleName),
            new Claim("RoleId", account.RoleId.ToString())
        };

        if (account.RoleId == AppRoles.EmployeeId && account.Employee != null)
        {
            claims.Add(new Claim("UserId", account.Employee.EmployeeId.ToString()));
            claims.Add(new Claim("FullName", account.Employee.FullName));
        }
        else if (account.Manager != null)
        {
            claims.Add(new Claim("UserId", account.Manager.ManagerId.ToString()));
            claims.Add(new Claim("FullName", account.Manager.FullName));
        }

        var expiryDays = double.Parse(jwtSettings["ExpiryDays"] ?? "7");
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"] ?? "QRDrinkOrderAPI",
            audience: jwtSettings["Audience"] ?? "QRDrinkOrderClient",
            claims: claims,
            expires: DateTime.Now.AddDays(expiryDays),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

