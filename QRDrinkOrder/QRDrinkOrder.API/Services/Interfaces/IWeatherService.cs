using QRDrinkOrder.API.Models.External;

namespace QRDrinkOrder.API.Services.Interfaces;

public interface IWeatherService
{
    Task<WeatherResponse?> GetCurrentWeatherAsync();
}
