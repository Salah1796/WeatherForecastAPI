using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Options;
using WeatherForecast.Domain.Entities;
using WeatherForecast.Infrastructure.DTOs;
using WeatherForecast.Infrastructure.Services;
using Xunit;

namespace WeatherForecast.Infrastructure.Tests.Services;

public class JwtTokenGeneratorTests
{
    private readonly JwtSettings _jwtSettings;
    private readonly JwtTokenGenerator _jwtTokenGenerator;

    public JwtTokenGeneratorTests()
    {
        _jwtSettings = new JwtSettings
        {
            SecretKey = "super_secret_key_for_testing_purposes_only",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 60
        };

        var optionsMock = Options.Create(_jwtSettings);
        _jwtTokenGenerator = new JwtTokenGenerator(optionsMock);
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidJwtToken()
    {
        // Arrange
        var user = new User("testuser", "hash");

        // Act
        var tokenString = _jwtTokenGenerator.GenerateToken(user);

        // Assert
        tokenString.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);

        token.Issuer.Should().Be(_jwtSettings.Issuer);
        token.Audiences.Should().Contain(_jwtSettings.Audience);
        token.Claims.Should().Contain(c => (c.Type  == System.Security.Claims.ClaimTypes.Name) && c.Value == user.Username);
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new JwtTokenGenerator(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithEmptySecretKey_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var settings = new JwtSettings { SecretKey = "" };
        var options = Options.Create(settings);

        // Act
        var act = () => new JwtTokenGenerator(options);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
