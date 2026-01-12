using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using WeatherForecast.Application.Common.Enums;
using WeatherForecast.Application.Common.Localization;
using WeatherForecast.Application.Common.Results;
using WeatherForecast.Application.DTOs;
using WeatherForecast.Application.Interfaces;

namespace WeatherForecast.Infrastructure.Services;

/// <summary>
/// Decorator for weather service that adds caching functionality.
/// </summary>
public class CachedWeatherService : IWeatherService
{
    private readonly IWeatherService _weatherService;
    private readonly IMemoryCache _cache;
    private readonly int _cacheTTLMinutes;
    private readonly IAppLocalizer _localizer;


    /// <summary>
    /// Initializes a new instance of the CachedWeatherService class with the specified weather service, memory cache,
    /// configuration, and localizer.
    /// </summary>
    /// <param name="weatherService">The underlying weather service used to retrieve weather data when it is not available in the cache. Cannot be
    /// null.</param>
    /// <param name="cache">The memory cache instance used to store and retrieve cached weather data. Cannot be null.</param>
    /// <param name="configuration">The configuration provider used to obtain cache settings, such as the cache time-to-live for weather data.
    /// Cannot be null.</param>
    /// <param name="localizer">The localizer used for providing localized strings and messages. Cannot be null.</param>
    public CachedWeatherService(
        IWeatherService weatherService,
        IMemoryCache cache,
        IConfiguration configuration,
        IAppLocalizer localizer)
    {
        _weatherService = weatherService;
        _cache = cache;
        var cacheSettings = configuration.GetSection("CacheSettings");
        _cacheTTLMinutes = int.Parse(cacheSettings["WeatherCacheTTLMinutes"] ?? "30");
        _localizer = localizer;
    }

    /// <summary>
    /// Retrieves weather forecast data for a specific city with caching.
    /// </summary>
    /// <param name="city">The name of the city to get weather data for.</param>
    /// <returns>A result containing the weather response with forecast data if successful, or error details if failed.</returns>
    public async Task<Result<WeatherResponse>> GetWeatherByCityAsync(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return Result<WeatherResponse>.ErrorResponse(_localizer["CityNameRequired"], StatusCode.BadRequest);
        }

        var cacheKey = $"weather_{city.ToLowerInvariant()}";

        if (_cache.TryGetValue<Result<WeatherResponse>>(cacheKey, out var cachedResult))
        {
            return cachedResult!;
        }

        var result = await _weatherService.GetWeatherByCityAsync(city);

        if (result.Success && result.Data != null)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheTTLMinutes)
            };

            _cache.Set(cacheKey, result, cacheOptions);
        }

        return result;
    }
}

