using WeatherForecast.Application.Common.Results;
using WeatherForecast.Application.DTOs;

namespace WeatherForecast.Application.Interfaces;

/// <summary>
/// Interface for weather service operations.
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Retrieves weather forecast data for a specific city.
    /// </summary>
    /// <param name="city">The name of the city to get weather data for.</param>
    /// <returns>A result containing the weather response with forecast data if successful, or error details if failed.</returns>
    Task<Result<WeatherResponse>> GetWeatherByCityAsync(string city);
}

