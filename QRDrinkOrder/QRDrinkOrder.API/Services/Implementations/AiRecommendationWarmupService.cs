using QRDrinkOrder.API.Services.Interfaces;

namespace QRDrinkOrder.API.Services.Implementations;

public class AiRecommendationWarmupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AiRecommendationWarmupService> _logger;
    private readonly TimeSpan _warmupInterval = TimeSpan.FromMinutes(30);

    public AiRecommendationWarmupService(
        IServiceProvider serviceProvider,
        ILogger<AiRecommendationWarmupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AiRecommendationWarmupService is starting.");

        // Chờ 10 giây sau khi server khởi động để đảm bảo DB và các dịch vụ đã sẵn sàng
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("AiRecommendationWarmupService: Bắt đầu làm mới cache AI Recommendation (Pre-computation)...");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var aiService = scope.ServiceProvider.GetRequiredService<IAiRecommendationService>();
                    // Gọi ép làm mới cache (isForceRefresh: true)
                    await aiService.GetDrinkRecommendationsAsync(isForceRefresh: true);
                }

                _logger.LogInformation("AiRecommendationWarmupService: Làm mới cache AI Recommendation thành công! Chu kỳ tiếp theo sau 30 phút.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AiRecommendationWarmupService: Lỗi xảy ra trong quá trình làm mới cache AI Recommendation.");
            }

            // Đợi 30 phút cho lần warmup tiếp theo
            await Task.Delay(_warmupInterval, stoppingToken);
        }

        _logger.LogInformation("AiRecommendationWarmupService is stopping.");
    }
}
