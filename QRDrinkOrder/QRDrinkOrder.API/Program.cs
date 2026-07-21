using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QRDrinkOrder.API.Hubs;
using QRDrinkOrder.API.Middlewares;
using QRDrinkOrder.API.Models;
using QRDrinkOrder.API.Services.Implementations;
using QRDrinkOrder.API.Services.Interfaces;
using System.Text;
using System.Threading.RateLimiting;

namespace QRDrinkOrder.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
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
            builder.Services.AddHostedService<AiRecommendationWarmupService>();
            builder.Services.AddScoped<IImageService, CloudinaryImageService>();

            // 4. Đăng ký SignalR để xử lý thông báo thời gian thực
            builder.Services.AddSignalR();

            // 5. Cấu hình CORS để cho phép ứng dụng Blazor WebAssembly kết nối
            builder.Services.AddCors(options =>
            {
                var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.SetIsOriginAllowed(origin =>
                          {
                              if (string.IsNullOrEmpty(origin)) return false;
                              if (origin.StartsWith("http://localhost:") || origin.StartsWith("https://localhost:")) return true;
                              if (origin.EndsWith(".vercel.app") || origin.EndsWith(".onrender.com")) return true;
                              return allowedOrigins.Any(o => string.Equals(origin.TrimEnd('/'), o.TrimEnd('/'), StringComparison.OrdinalIgnoreCase));
                          })
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
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
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "QRDrinkOrder API",
                    Version = "v1",
                    Description = "API cho hệ thống đặt nước QR Drink Order V2 (Khách hàng, Nhân viên, Ban quản lý)"
                });

                // Cấu hình nút Authorize nhập JWT Token trong Swagger UI
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Nhập token JWT vào ô bên dưới (không cần gõ chữ Bearer, hệ thống tự động nhận diện)"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

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

                options.AddPolicy("LookupLimiter", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 30,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));
                options.RejectionStatusCode = 429;
            });

            var app = builder.Build();

            app.UseForwardedHeaders();

            // Áp dụng CORS trước tất cả các Middleware khác (đặc biệt là GlobalExceptionMiddleware) để đảm bảo lỗi 500/400 vẫn có header CORS
            app.UseCors("CorsPolicy");

            // Thêm Middleware xử lý lỗi tập trung
            app.UseMiddleware<GlobalExceptionMiddleware>();

            // Cấu hình đường ống HTTP (Request Pipeline)
            if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("EnableSwagger", true))
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "QRDrinkOrder API v1");
                    c.RoutePrefix = "swagger"; // Mở tại /swagger
                });
            }

            // app.UseHttpsRedirection(); // Tắt dòng này khi deploy lên Render vì Render đã xử lý HTTPS

            // Kích hoạt phục vụ file tĩnh trong wwwroot (ảnh đồ uống, ảnh review)
            app.UseStaticFiles();

            app.UseRateLimiter();

            app.UseAuthentication();
            app.UseAuthorization();

            // Ánh xạ API Controllers và SignalR Hub
            app.MapControllers();
            app.MapHub<OrderHub>("/orderhub");

            // Thêm Health Check endpoint để Render kiểm tra tình trạng server (tránh lỗi 404 khi HEAD/GET trang chủ khiến Render tự tắt container)
            app.MapMethods("/", new[] { "GET", "HEAD" }, () => Results.Ok(new { status = "online", service = "QRDrinkOrder API v1", timestamp = DateTime.UtcNow }));
            app.MapMethods("/health", new[] { "GET", "HEAD" }, () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

            app.Run();
        }
    }
}
