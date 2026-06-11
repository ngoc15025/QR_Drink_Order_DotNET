using System.Text.Json.Serialization;

namespace QRDrinkOrder.API.Models.External;

public class WeatherResponse
{
    [JsonPropertyName("weather")]
    public List<WeatherInfo>? Weather { get; set; }

    [JsonPropertyName("main")]
    public MainInfo? Main { get; set; }
}

public class WeatherInfo
{
    [JsonPropertyName("main")]
    public string? Main { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public class MainInfo
{
    [JsonPropertyName("temp")]
    public double Temp { get; set; }
}
