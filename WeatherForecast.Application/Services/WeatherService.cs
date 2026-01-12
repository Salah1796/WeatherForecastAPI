using WeatherForecast.Application.Common.Enums;
using WeatherForecast.Application.Common.Localization;
using WeatherForecast.Application.Common.Results;
using WeatherForecast.Application.DTOs;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Domain.Repositories;

namespace WeatherForecast.Application.Services;

/// <summary>
/// Service for retrieving weather forecast data for cities.
/// </summary>
public class WeatherService : IWeatherService
{
    private readonly IWeatherRepository _weatherRepository;
    private readonly IAppLocalizer _localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeatherService"/> class.
    /// </summary>
    /// <param name="weatherRepository">The weather repository for data access operations.</param>
    /// <param name="localizer">The localizer for retrieving localized messages.</param>
    public WeatherService(IWeatherRepository weatherRepository, IAppLocalizer localizer)
    {
        _weatherRepository = weatherRepository;
        _localizer = localizer;
    }

    /// <summary>
    /// Retrieves weather forecast data for a specific city.
    /// </summary>
    /// <param name="city">The name of the city to get weather data for.</param>
    /// <returns>A result containing the weather response with forecast data if successful, or error details if failed.</returns>
    public async Task<Result<WeatherResponse>> GetWeatherByCityAsync(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            return Result<WeatherResponse>.ErrorResponse(_localizer["CityNameRequired"], StatusCode.BadRequest);

        var weatherForecast = await _weatherRepository.GetByCityAsync(city);

        if (weatherForecast == null)
            return Result<WeatherResponse>.ErrorResponse(_localizer["WeatherDataNotFound"], StatusCode.NotFound);

        var weatherResponse = new WeatherResponse
        {
            City = weatherForecast.City,
            Temperature = weatherForecast.Temperature,
            Condition = weatherForecast.Condition
        };

        return Result<WeatherResponse>.SuccessResponse(weatherResponse, _localizer["WeatherDataRetrievedSuccessfully"]);
    }
}

