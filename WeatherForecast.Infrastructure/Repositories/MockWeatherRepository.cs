#pragma warning disable CA1873 // Avoid potentially expensive logging

using Microsoft.Extensions.Logging;
using System.Text.Json;
using WeatherForecast.Domain.Repositories;
using WeatherForecast.Infrastructure.DTOs;
using WeatherForecastValueObject = WeatherForecast.Domain.ValueObjects.WeatherForecast;

namespace WeatherForecast.Infrastructure.Repositories;

/// <summary>
/// Mock implementation of the weather repository that loads data from JSON file.
/// </summary>
public class MockWeatherRepository : IWeatherRepository
{
    // Initialize dictionary with case-insensitive comparer to match constructor usage
    private readonly Lazy<Dictionary<string, WeatherForecastValueObject>> _weatherData;
    private readonly ILogger<MockWeatherRepository>? _logger;


    /// <summary>
    /// Initializes a new instance of the <see cref="MockWeatherRepository"/> class.
    /// </summary>
    public MockWeatherRepository(ILogger<MockWeatherRepository> logger)
    {
        _logger = logger;
        _weatherData = new Lazy<Dictionary<string, WeatherForecastValueObject>>(LoadData);
    }

    private Dictionary<string, WeatherForecastValueObject> LoadData()
    {
        var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "weather-data.json");
        var emptyResult = new Dictionary<string, WeatherForecastValueObject>(StringComparer.OrdinalIgnoreCase);

        if (!File.Exists(jsonPath))
        {
            _logger?.LogError("Weather data file not found at path: {JsonPath}", jsonPath);
            return emptyResult;
        }

        var jsonContent = File.ReadAllText(jsonPath);
        
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            _logger?.LogError("Weather data file is empty at path: {JsonPath}", jsonPath);
            return emptyResult;
        }

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var weatherItems = JsonSerializer.Deserialize<List<WeatherForecastDto>>(jsonContent, options);
            if (weatherItems == null || weatherItems.Count == 0)
            {
                _logger?.LogError("No weather data found in the JSON file at path: {JsonPath}", jsonPath);
                return emptyResult;
            }

            return weatherItems.ToDictionary(
                item => item.City,
                item => new WeatherForecastValueObject(item.City, item.Temperature, item.Condition), 
                StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "Error parsing weather data JSON at path: {JsonPath}", jsonPath);
            return emptyResult;
        }
    }

    /// <summary>
    /// Gets a weather forecast by city.
    /// </summary>
    /// <param name="city">The city to get the weather forecast for.</param>
    /// <returns>The weather forecast for the specified city.</returns>
    public Task<WeatherForecastValueObject?> GetByCityAsync(string city)
    {
        if (city == null) return Task.FromResult<WeatherForecastValueObject?>(null);

        if (_weatherData.Value.TryGetValue(city, out var weather))
        {
            return Task.FromResult<WeatherForecastValueObject?>(weather);
        }

        return Task.FromResult<WeatherForecastValueObject?>(null);
    }

}
