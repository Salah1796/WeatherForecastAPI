using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using WeatherForecast.Application.Common.Enums;
using WeatherForecast.Application.Common.Localization;
using WeatherForecast.Application.Common.Results;
using WeatherForecast.Application.DTOs;
using WeatherForecast.Application.Interfaces;

namespace WeatherForecast.Infrastructure.Services;

/// <summary>
/// Redis implementation of weather service caching.
/// </summary>
public class RedisWeatherCacheService : IWeatherService
{
    private readonly IWeatherService _weatherService;
    private readonly IDistributedCache _cache;
    private readonly int _cacheTTLMinutes;
    private readonly IAppLocalizer _localizer;

    public RedisWeatherCacheService(
        IWeatherService weatherService,
        IDistributedCache cache,
        IConfiguration configuration,
        IAppLocalizer localizer)
    {
        _weatherService = weatherService;
        _cache = cache;
        var cacheSettings = configuration.GetSection("CacheSettings");
        _cacheTTLMinutes = int.Parse(cacheSettings["RedisCacheTTLMinutes"] ?? "60");
        _localizer = localizer;
    }

    public async Task<Result<WeatherResponse>> GetWeatherByCityAsync(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return Result<WeatherResponse>.ErrorResponse(_localizer["CityNameRequired"], StatusCode.BadRequest);
        }

        var cacheKey = $"weather_redis_{city.ToLowerInvariant()}";

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            var cachedResult = JsonSerializer.Deserialize<Result<WeatherResponse>>(cachedData);
            if (cachedResult != null)
            {
                return cachedResult;
            }
        }

        var result = await _weatherService.GetWeatherByCityAsync(city);

        if (result.Success && result.Data != null)
        {
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheTTLMinutes)
            };

            var serializedResult = JsonSerializer.Serialize(result);
            await _cache.SetStringAsync(cacheKey, serializedResult, cacheOptions);
        }

        return result;
    }
}
