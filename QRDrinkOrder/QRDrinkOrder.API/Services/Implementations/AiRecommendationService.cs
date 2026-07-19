using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QRDrinkOrder.API.Models;
using QRDrinkOrder.API.Models.External;
using QRDrinkOrder.API.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace QRDrinkOrder.API.Services.Implementations;

public class AiRecommendationService : IAiRecommendationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IWeatherService _weatherService;
    private readonly QrdrinkOrderDbContext _context;
    private readonly ILogger<AiRecommendationService> _logger;
    private readonly IMemoryCache _cache;

    public AiRecommendationService(
        HttpClient httpClient,
        IConfiguration configuration,
        IWeatherService weatherService,
        QrdrinkOrderDbContext context,
        ILogger<AiRecommendationService> logger,
        IMemoryCache cache)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _weatherService = weatherService;
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<AiRecommendationResult> GetDrinkRecommendationsAsync(bool isForceRefresh = false)
    {
        const string cacheKey = "AiRecommendation_Latest";
        if (!isForceRefresh && _cache.TryGetValue(cacheKey, out AiRecommendationResult? cachedResult) && cachedResult != null)
        {
            return cachedResult;
        }

        var result = new AiRecommendationResult();

        try
        {
            // 1 & 2. Get current weather and fetch active drinks from DB in parallel
            var weatherTask = _weatherService.GetCurrentWeatherAsync();
            var drinksTask = _context.Drinks
                .Include(d => d.DrinkTranslations)
                .Where(d => d.IsActive == true)
                .Select(d => new
                {
                    Id = d.DrinkId,
                    Name = d.DrinkTranslations.FirstOrDefault(t => t.LanguageCode == "vi") != null
                           ? d.DrinkTranslations.First(t => t.LanguageCode == "vi").DrinkName
                           : "Unknown",
                    TemperatureType = d.TemperatureType.ToString()
                })
                .ToListAsync();

            await Task.WhenAll(weatherTask, drinksTask);
            var weather = await weatherTask;
            var activeDrinks = await drinksTask;

            var locationName = !string.IsNullOrEmpty(weather?.Name) ? weather.Name : "Quận 8, TP. Hồ Chí Minh";
            var weatherDescription = $"tại khu vực {locationName}: thời tiết mát mẻ bình thường";
            if (weather?.Weather != null && weather.Weather.Any() && weather.Main != null)
            {
                weatherDescription = $"tại khu vực {locationName}: nhiệt độ hiện tại {weather.Main.Temp}°C, {weather.Weather[0].Description}";
            }

            if (!activeDrinks.Any())
            {
                result.Message = "Hiện tại quán chưa có món nào để gợi ý.";
                return result;
            }

            var drinksJson = JsonSerializer.Serialize(activeDrinks);

            // 3. Build Prompt for Gemini
            var prompt = $@"
Bạn là một nhân viên phục vụ vui tính và thân thiện tại quán Ngoc UwU Coffee (Chi nhánh Quận 8, TP. Hồ Chí Minh).
Tình hình thời tiết {weatherDescription}.
Dưới đây là danh sách các món nước quán đang bán (ID, Tên món, Loại nhiệt độ Hot/Iced/Both/Other):
{drinksJson}

Dựa vào tình hình thời tiết và nhiệt độ thực tế tại Quận 8, Hồ Chí Minh lúc này, hãy chọn ra đúng 3 món phù hợp nhất để giới thiệu cho khách hàng (trời nóng bức thì ưu tiên đồ lạnh giải khát, trời mát hoặc có mưa thì ưu tiên đồ ấm hoặc trà sữa thơm béo).
Trả về KẾT QUẢ ĐÚNG CHUẨN JSON sau:
{{
  ""message"": ""1 câu chào mời ngắn gọn (dưới 25 từ), vui vẻ, tự nhiên, có nhắc khéo đến thời tiết/nhiệt độ tại Quận 8 (ví dụ: Trời Quận 8 đang nóng 34°C, làm ngay ly trà mát lạnh giải nhiệt nha bạn ơi!)"",
  ""drinkIds"": [ID món 1, ID món 2, ID món 3]
}}
Không trả về bất kỳ text nào ngoài JSON. Không bọc JSON trong dấu markdown ```json.
";

            // 4. Call Gemini API
            var apiKey = _configuration["ExternalApis:Gemini:ApiKey"];
            var model = _configuration["ExternalApis:Gemini:Model"] ?? "gemini-1.5-pro";
            var geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            var requestBody = new GeminiRequest
            {
                Contents = new List<GeminiContent>
                {
                    new GeminiContent
                    {
                        Parts = new List<GeminiPart> { new GeminiPart { Text = prompt } }
                    }
                },
                GenerationConfig = new GeminiGenerationConfig
                {
                    ResponseMimeType = "application/json"
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(geminiUrl, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent);

                var generatedText = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                if (!string.IsNullOrEmpty(generatedText))
                {
                    try
                    {
                        var cleanJson = generatedText;
                        var startIndex = generatedText.IndexOf('{');
                        var endIndex = generatedText.LastIndexOf('}');
                        if (startIndex >= 0 && endIndex > startIndex)
                        {
                            cleanJson = generatedText.Substring(startIndex, endIndex - startIndex + 1);
                        }

                        var parsedResult = JsonSerializer.Deserialize<AiRecommendationResult>(cleanJson, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (parsedResult != null)
                        {
                            // Lưu vào cache 45 phút (phù hợp với chu kỳ làm mới mỗi 30 phút của BackgroundService)
                            _cache.Set(cacheKey, parsedResult, TimeSpan.FromMinutes(45));
                            return parsedResult;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to parse JSON from Gemini response: " + generatedText);
                    }
                }
            }
            else
            {
                _logger.LogWarning($"Gemini API failed with status: {response.StatusCode}. Response: {await response.Content.ReadAsStringAsync()}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI recommendations");
        }

        // Fallback if AI fails
        result.Message = "Hôm nay bạn muốn uống gì nào? Tham khảo thử các món này nhé!";
        var defaultDrinks = await _context.Drinks.Where(d => d.IsActive == true).Take(3).Select(d => d.DrinkId).ToListAsync();
        result.DrinkIds = defaultDrinks;
        return result;
    }
}
