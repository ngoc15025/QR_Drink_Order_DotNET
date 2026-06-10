using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QRDrinkOrder.API.Hubs;
using QRDrinkOrder.API.Services.Implementations;
using QRDrinkOrder.API.Services.Interfaces;
using QRDrinkOrder.Shared.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký Controllers
builder.Services.AddControllers();

// 2. Đăng ký Database Context (SQL Server)
builder.Services.AddDbContext<QrdrinkOrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Đăng ký các dịch vụ Nghiệp vụ (Business Services)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IMembershipService, MembershipService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// 4. Đăng ký SignalR để xử lý thông báo thời gian thực
builder.Services.AddSignalR();

// 5. Cấu hình CORS để cho phép ứng dụng Blazor WebAssembly kết nối
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
                policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Bắt buộc cho SignalR kết nối thời gian thực
    });
});

// 6. Cấu hình xác thực JWT Bearer
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? "QRDrinkOrderSecureKey2026_LongEnoughToMeetRequirements_MustBeAtLeast32Bytes";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "QRDrinkOrderAPI",
        ValidAudience = jwtSettings["Audience"] ?? "QRDrinkOrderClient",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// 7. Đăng ký OpenAPI/Swagger để kiểm thử API
builder.Services.AddOpenApi();

var app = builder.Build();

// Cấu hình đường ống HTTP (Request Pipeline)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}



// Kích hoạt phục vụ file tĩnh trong wwwroot (ảnh đồ uống, ảnh review)
app.UseStaticFiles();

// Áp dụng CORS trước Authentication & Authorization
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Ánh xạ API Controllers và SignalR Hub
app.MapControllers();
app.MapHub<OrderHub>("/orderhub");

app.Run();
