using WeatherForecastValueObject = WeatherForecast.Domain.ValueObjects.WeatherForecast;

namespace WeatherForecast.Domain.Repositories;

/// <summary>
/// Interface for the weather repository.
/// </summary>
public interface IWeatherRepository
{
    /// <summary>
    /// Gets a weather forecast by city.
    /// </summary>
    /// <param name="city">The city to get the weather forecast for.</param>
    /// <returns>The weather forecast for the specified city.</returns>
    Task<WeatherForecastValueObject?> GetByCityAsync(string city);
}