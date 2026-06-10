using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using QRDrinkOrder.API.Models.External;
using QRDrinkOrder.API.Services.Interfaces;

namespace QRDrinkOrder.API.Services.Implementations;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WeatherService> _logger;

    private const string CacheKey = "CurrentWeather";

    public WeatherService(
        HttpClient httpClient, 
        IConfiguration configuration,
        IMemoryCache cache,
        ILogger<WeatherService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _cache = cache;
        _logger = logger;
    }

    public async Task<WeatherResponse?> GetCurrentWeatherAsync()
    {
        if (_cache.TryGetValue(CacheKey, out WeatherResponse? cachedWeather))
        {
            return cachedWeather;
        }

        var apiKey = _configuration["ExternalApis:OpenWeatherMap:ApiKey"];
        var city = _configuration["ExternalApis:OpenWeatherMap:City"] ?? "Ho Chi Minh City";

        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_OPENWEATHER_KEY")
        {
            _logger.LogWarning("OpenWeatherMap API Key is not configured properly.");
            return null;
        }

        try
        {
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(city)}&appid={apiKey}&units=metric";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var weatherData = JsonSerializer.Deserialize<WeatherResponse>(content);
                
                if (weatherData != null)
                {
                    // Cache the weather data for 30 minutes
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
                    
                    _cache.Set(CacheKey, weatherData, cacheEntryOptions);
                    return weatherData;
                }
            }
            else
            {
                _logger.LogWarning($"Failed to get weather data. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching weather data");
        }

        return null;
    }
}
