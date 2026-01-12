using Microsoft.Extensions.Localization;
using Moq;
using WeatherForecast.Application.Common.Enums;
using WeatherForecast.Application.Common.Localization;
using WeatherForecast.Application.Services;
using WeatherForecast.Domain.Repositories;
using Xunit;
using WeatherForecastValueObject = WeatherForecast.Domain.ValueObjects.WeatherForecast;

namespace WeatherForecast.Application.Tests.Services;

public class WeatherServiceTests
{
    private readonly Mock<IWeatherRepository> _weatherRepositoryMock;
    private readonly Mock<IAppLocalizer> _localizerMock;
    private readonly WeatherService _weatherService;

    public WeatherServiceTests()
    {
        _weatherRepositoryMock = new Mock<IWeatherRepository>();
        _localizerMock = new Mock<IAppLocalizer>();

        // Setup localizer to return the key as the value
        _localizerMock.Setup(x => x[It.IsAny<string>()])
            .Returns((string key) => key);

        _weatherService = new WeatherService(_weatherRepositoryMock.Object, _localizerMock.Object);
    }

    [Fact]
    public async Task GetWeatherByCityAsync_WithValidCity_ReturnsSuccessResult()
    {
        // Arrange
        const string city = "Cairo";
        var weatherForecast = new WeatherForecastValueObject(city, 15.5, "Partly Cloudy");

        _weatherRepositoryMock.Setup(x => x.GetByCityAsync(city))
            .ReturnsAsync(weatherForecast);

        // Act
        var result = await _weatherService.GetWeatherByCityAsync(city);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(city, result.Data.City);
        Assert.Equal(15.5, result.Data.Temperature);
        Assert.Equal("Partly Cloudy", result.Data.Condition);
        Assert.Equal(StatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task GetWeatherByCityAsync_WithNullCity_ReturnsBadRequestError()
    {
        // Act
        var result = await _weatherService.GetWeatherByCityAsync(null!);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(StatusCode.BadRequest, result.StatusCode);
        Assert.Equal(_localizerMock.Object["CityNameRequired"], result.Message);
    }

    [Fact]
    public async Task GetWeatherByCityAsync_WithEmptyCity_ReturnsBadRequestError()
    {
        // Act
        var result = await _weatherService.GetWeatherByCityAsync("");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(StatusCode.BadRequest, result.StatusCode);
        Assert.Equal(_localizerMock.Object["CityNameRequired"], result.Message);
    }

    [Fact]
    public async Task GetWeatherByCityAsync_WithWhitespaceCity_ReturnsBadRequestError()
    {
        // Act
        var result = await _weatherService.GetWeatherByCityAsync("   ");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(StatusCode.BadRequest, result.StatusCode);
        Assert.Equal(_localizerMock.Object["CityNameRequired"], result.Message);
    }

    [Fact]
    public async Task GetWeatherByCityAsync_WithCityNotFound_ReturnsNotFoundError()
    {
        // Arrange
        const string city = "NonExistentCity";

        _weatherRepositoryMock.Setup(x => x.GetByCityAsync(city))
            .ReturnsAsync((WeatherForecastValueObject?)null);

        // Act
        var result = await _weatherService.GetWeatherByCityAsync(city);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(StatusCode.NotFound, result.StatusCode);
        Assert.Null(result.Data);
        Assert.Equal(_localizerMock.Object["WeatherDataNotFound"], result.Message);
    }
}
