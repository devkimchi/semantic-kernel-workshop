using System.ComponentModel;

using Microsoft.SemanticKernel;

namespace Workshop.ConsoleApp.Plugins.WeatherBot;

public class WeatherPlugin
{
    private static readonly Dictionary<string, WeatherData> weathers = new(StringComparer.InvariantCultureIgnoreCase)
    {
        { "London", new WeatherData(12.5f, 75, "cloudy") },
        { "New York", new WeatherData(22.1f, 60, "sunny") },
        { "Tokyo", new WeatherData(18.3f, 85, "rainy") },
        { "Seoul", new WeatherData(22.1f, 60, "sunny") },
        { "Paris", new WeatherData(14.0f, 70, "partly cloudy") },
        { "Sydney", new WeatherData(25.0f, 50, "sunny") }
    };

    [KernelFunction("get_weather")]
    [Description("Gets the current weather details for a city")]
    public static string GetWeather(string city)
    {
        return weathers.TryGetValue(city, out WeatherData? data)
            ? $"Weather forecast for {city}:\n" +
                   $"Temperature: {data.Temperature}°C\n" +
                   $"Humidity: {data.Humidity}%\n" +
                   $"Condition: {data.Condition}"
            : $"Sorry, we do not have weather data for {city}.";
    }
}

public class WeatherData(float temperature, int humidity, string condition)
{
    public float Temperature { get; set; } = temperature;
    public int Humidity { get; set; } = humidity;
    public string Condition { get; set; } = condition;
}
