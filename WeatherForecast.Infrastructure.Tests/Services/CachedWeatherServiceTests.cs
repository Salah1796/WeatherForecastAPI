using Microsoft.Extensions.Caching.Memory;
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

public class CachedWeatherServiceTests
{
    private readonly Mock<IWeatherService> _weatherServiceMock;
    private readonly IMemoryCache _cache;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IAppLocalizer> _localizerMock;
    private readonly CachedWeatherService _cachedWeatherService;

    public CachedWeatherServiceTests()
    {
        _weatherServiceMock = new Mock<IWeatherService>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _configurationMock = new Mock<IConfiguration>();
        _localizerMock = new Mock<IAppLocalizer>();

        var cacheSectionMock = new Mock<IConfigurationSection>();
        cacheSectionMock.Setup(s => s["WeatherCacheTTLMinutes"]).Returns("30");
        _configurationMock.Setup(c => c.GetSection("CacheSettings")).Returns(cacheSectionMock.Object);

        _cachedWeatherService = new CachedWeatherService(
            _weatherServiceMock.Object,
            _cache,
            _configurationMock.Object,
            _localizerMock.Object);
    }

    [Fact]
    public async Task GetWeatherByCityAsync_WhenInCache_ShouldReturnCachedValue()
    {
        // Arrange
        var city = "Cairo";
        var cacheKey = $"weather_cairo";
        var cachedResult = Result<WeatherResponse>.SuccessResponse(
            new WeatherResponse { City = "Cairo", Temperature = 20, Condition = "Sunny" }, "Cached");
        
        _cache.Set(cacheKey, cachedResult);

        // Act
        var result = await _cachedWeatherService.GetWeatherByCityAsync(city);

        // Assert
        Assert.Same(cachedResult, result);
        _weatherServiceMock.Verify(s => s.GetWeatherByCityAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetWeatherByCityAsync_WhenNotInCache_ShouldCallUnderlyingServiceAndCache()
    {
        // Arrange
        var city = "London";
        var serviceResult = Result<WeatherResponse>.SuccessResponse(
            new WeatherResponse { City = "London", Temperature = 15, Condition = "Cloudy" }, "Fresh");
        
        _weatherServiceMock.Setup(s => s.GetWeatherByCityAsync(city))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _cachedWeatherService.GetWeatherByCityAsync(city);

        // Assert
        Assert.Same(serviceResult, result);
        _weatherServiceMock.Verify(s => s.GetWeatherByCityAsync(city), Times.Once);

        // Check if it was cached
        var cacheKey = $"weather_london";
        Assert.True(_cache.TryGetValue(cacheKey, out Result<WeatherResponse>? cachedValue));
        Assert.Same(serviceResult, cachedValue);
    }

    [Fact]
    public async Task GetWeatherByCityAsync_WhenServiceFails_ShouldNotCache()
    {
        // Arrange
        var city = "ErrorCity";
        var serviceResult = Result<WeatherResponse>.ErrorResponse("Error", StatusCode.NotFound);
        
        _weatherServiceMock.Setup(s => s.GetWeatherByCityAsync(city))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _cachedWeatherService.GetWeatherByCityAsync(city);

        // Assert
        Assert.Same(serviceResult, result);
        var cacheKey = $"weather_errorcity";
        Assert.False(_cache.TryGetValue(cacheKey, out _));
    }
}
