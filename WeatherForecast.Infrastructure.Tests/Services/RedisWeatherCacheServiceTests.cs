using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Moq;
using WeatherForecast.Application.Common.Enums;
using WeatherForecast.Application.Common.Localization;
using WeatherForecast.Application.Common.Results;
using WeatherForecast.Application.DTOs;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Infrastructure.Services;
using Xunit;

namespace WeatherForecast.Infrastructure.Tests.Services;

public class RedisWeatherCacheServiceTests
{
    private readonly Mock<IWeatherService> _weatherServiceMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IAppLocalizer> _localizerMock;
    private readonly RedisWeatherCacheService _redisWeatherCacheService;

    public RedisWeatherCacheServiceTests()
    {
        _weatherServiceMock = new Mock<IWeatherService>();
        _cacheMock = new Mock<IDistributedCache>();
        _configurationMock = new Mock<IConfiguration>();
        _localizerMock = new Mock<IAppLocalizer>();

        var cacheSectionMock = new Mock<IConfigurationSection>();
        cacheSectionMock.Setup(s => s["RedisCacheTTLMinutes"]).Returns("60");
        _configurationMock.Setup(c => c.GetSection("CacheSettings")).Returns(cacheSectionMock.Object);

        _redisWeatherCacheService = new RedisWeatherCacheService(
            _weatherServiceMock.Object,
            _cacheMock.Object,
            _configurationMock.Object,
            _localizerMock.Object);
    }

    [Fact]
    public async Task GetWeatherByCityAsync_WhenInCache_ShouldReturnCachedValue()
    {
        // Arrange
        var city = "Cairo";
        var cacheKey = $"weather_redis_cairo";
        var cachedResult = Result<WeatherResponse>.SuccessResponse(
            new WeatherResponse { City = "Cairo", Temperature = 20, Condition = "Sunny" }, "Cached");
        var serializedResult = JsonSerializer.Serialize(cachedResult);
        
        _cacheMock.Setup(c => c.GetAsync(cacheKey, default))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes(serializedResult));

        // Act
        var result = await _redisWeatherCacheService.GetWeatherByCityAsync(city);

        // Assert
        Assert.Equal(cachedResult.Data.City, result.Data!.City);
        Assert.Equal(cachedResult.Data.Temperature, result.Data.Temperature);
        _weatherServiceMock.Verify(s => s.GetWeatherByCityAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetWeatherByCityAsync_WhenNotInCache_ShouldCallUnderlyingServiceAndCache()
    {
        // Arrange
        var city = "London";
        var serviceResult = Result<WeatherResponse>.SuccessResponse(
            new WeatherResponse { City = "London", Temperature = 15, Condition = "Cloudy" }, "Fresh");
        
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync((byte[]?)null);
        
        _weatherServiceMock.Setup(s => s.GetWeatherByCityAsync(city))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _redisWeatherCacheService.GetWeatherByCityAsync(city);

        // Assert
        Assert.Equal(serviceResult.Data!.City, result.Data!.City);
        _weatherServiceMock.Verify(s => s.GetWeatherByCityAsync(city), Times.Once);
        _cacheMock.Verify(c => c.SetAsync(
            It.Is<string>(k => k.Contains("london")),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            default), Times.Once);
    }
}
