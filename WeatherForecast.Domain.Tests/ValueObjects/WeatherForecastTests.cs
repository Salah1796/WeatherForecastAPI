using FluentAssertions;
using WeatherForecastValueObject = WeatherForecast.Domain.ValueObjects.WeatherForecast;

namespace WeatherForecast.Domain.Tests.ValueObjects;

public class WeatherForecastTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateWeatherForecastSuccessfully()
    {
        // Arrange
        var city = "New York";
        var temperature = 72.5;
        var condition = "Sunny";

        // Act
        var forecast = new WeatherForecastValueObject(city, temperature, condition);

        // Assert
        forecast.Should().NotBeNull();
        forecast.City.Should().Be(city);
        forecast.Temperature.Should().Be(temperature);
        forecast.Condition.Should().Be(condition);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WithNullEmptyOrWhitespaceCity_ShouldThrowArgumentNullException(string? city)
    {
        // Arrange
        var temperature = 72.5;
        var condition = "Sunny";

        // Act
        var act = () => new WeatherForecastValueObject(city!, temperature, condition);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("city");
    }
 
}

