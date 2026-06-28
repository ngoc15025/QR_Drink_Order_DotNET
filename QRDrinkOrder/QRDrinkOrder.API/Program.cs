using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QRDrinkOrder.API.Hubs;
using QRDrinkOrder.API.Services.Implementations;
using QRDrinkOrder.API.Services.Interfaces;
using QRDrinkOrder.API.Models;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using QRDrinkOrder.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký Controllers
builder.Services.AddControllers();

// 2. Đăng ký Database Context (SQL Server) và Interceptor
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<QRDrinkOrder.API.Interceptors.AuditLogInterceptor>();

builder.Services.AddDbContext<QrdrinkOrderDbContext>((sp, options) =>
{
    var interceptor = sp.GetRequiredService<QRDrinkOrder.API.Interceptors.AuditLogInterceptor>();
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .AddInterceptors(interceptor);
});

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

// Đăng ký HttpClient và MemoryCache cho AI Service
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IAiRecommendationService, AiRecommendationService>();
builder.Services.AddScoped<IImageService, CloudinaryImageService>();

// 4. Đăng ký SignalR để xử lý thông báo thời gian thực
builder.Services.AddSignalR();

// 5. Cấu hình CORS để cho phép ứng dụng Blazor WebAssembly kết nối
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    options.AddPolicy("CorsPolicy", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Bắt buộc cho SignalR kết nối thời gian thực
        }
        else
        {
            // Dự phòng cho môi trường dev chưa cấu hình
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

// 6. Cấu hình xác thực JWT Bearer
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"];

if (string.IsNullOrEmpty(secretKey) || secretKey.StartsWith("YOUR_"))
{
    throw new InvalidOperationException("JWT SecretKey is missing or invalid in configuration. Please configure it in appsettings.json or environment variables.");
}

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
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/orderhub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// 7. Đăng ký OpenAPI/Swagger để kiểm thử API
builder.Services.AddOpenApi();

// 8. Cấu hình Forwarded Headers để nhận đúng Scheme (HTTPS) từ Render Proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Xóa danh sách mạng mặc định để nhận diện proxy của Render
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// 9. Cấu hình Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.AddPolicy("LoginLimiter", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
    options.RejectionStatusCode = 429;
});

var app = builder.Build();

// Thêm Middleware xử lý lỗi tập trung
app.UseMiddleware<GlobalExceptionMiddleware>();

// Cấu hình đường ống HTTP (Request Pipeline)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection(); // Tắt dòng này khi deploy lên Render vì Render đã xử lý HTTPS

app.UseForwardedHeaders();

// Kích hoạt phục vụ file tĩnh trong wwwroot (ảnh đồ uống, ảnh review)
app.UseStaticFiles();

// Áp dụng CORS trước Authentication & Authorization
app.UseCors("CorsPolicy");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Ánh xạ API Controllers và SignalR Hub
app.MapControllers();
app.MapHub<OrderHub>("/orderhub");

app.Run();
