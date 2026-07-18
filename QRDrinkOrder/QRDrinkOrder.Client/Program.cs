using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using QRDrinkOrder.Client.Services;
using QRDrinkOrder.Client.Services.ApiClients;
using QRDrinkOrder.Client.Services.State;

namespace QRDrinkOrder.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");

        // Đọc URL Backend từ appsettings.json (dễ thay đổi khi deploy mà không cần recompile)
        string backendApiUrl = builder.Configuration["BackendApiUrl"] ?? "http://localhost:5153";

        // HttpClient dùng cho toàn bộ hệ thống
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(backendApiUrl) });

        // Đăng ký dịch vụ LocalStorage và Auth
        builder.Services.AddScoped<LocalStorageService>();
        builder.Services.AddScoped<CustomerAuthService>();
        builder.Services.AddSingleton<OrderStateService>();
        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

        // Đăng ký các dịch vụ API Clients
        builder.Services.AddScoped<MenuApiClient>();
        builder.Services.AddScoped<OrderApiClient>();
        builder.Services.AddScoped<AuthApiClient>();
        builder.Services.AddScoped<CouponApiClient>();
        builder.Services.AddScoped<ReportApiClient>();
        builder.Services.AddScoped<BankAccountApiClient>();
        builder.Services.AddScoped<ReviewApiClient>();
        builder.Services.AddScoped<MembershipApiClient>();
        builder.Services.AddScoped<PushNotificationService>();
        builder.Services.AddScoped<SystemConfigApiClient>();
        builder.Services.AddScoped<AuditLogApiClient>();
        builder.Services.AddScoped<IAiRecommendationApiClient, AiRecommendationApiClient>();

        // Đăng ký trạng thái giỏ hàng toàn cục
        builder.Services.AddScoped<CartState>();
        // Đăng ký Localization
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

        // Cấu hình ẩn log info của Authorization để console đỡ rối
        builder.Logging.AddFilter("Microsoft.AspNetCore.Authorization", Microsoft.Extensions.Logging.LogLevel.Warning);

        var host = builder.Build();

        // Cấu hình ngôn ngữ từ JSInterop
        var js = host.Services.GetRequiredService<IJSRuntime>();
        var result = await js.InvokeAsync<string>("localStorage.getItem", "blazorCulture");
        var culture = result != null ? new System.Globalization.CultureInfo(result) : new System.Globalization.CultureInfo("vi-VN");
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;

        // Tự động khởi tạo Firebase Auth từ cấu hình appsettings (nếu được inject khi build Vercel/Render)
        var firebaseConfig = builder.Configuration.GetSection("Firebase");
        if (firebaseConfig != null && !string.IsNullOrEmpty(firebaseConfig["ApiKey"]))
        {
            try
            {
                await js.InvokeVoidAsync("window.firebasePinAuth.init", new
                {
                    apiKey = firebaseConfig["ApiKey"],
                    authDomain = firebaseConfig["AuthDomain"],
                    projectId = firebaseConfig["ProjectId"],
                    storageBucket = firebaseConfig["StorageBucket"],
                    messagingSenderId = firebaseConfig["MessagingSenderId"],
                    appId = firebaseConfig["AppId"]
                });
            }
            catch
            {
                // Bỏ qua nếu JSInterop chưa sẵn sàng lúc khởi tạo
            }
        }

        await host.RunAsync();
    }
}
