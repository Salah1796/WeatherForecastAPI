using System.Net;
using WeatherForecast.Api.Tests.Fixtures;
using WeatherForecast.Application.Common.Enums;
using WeatherForecast.Application.Common.Results;
using WeatherForecast.Application.DTOs;
using Xunit;

namespace WeatherForecast.Api.Tests.Integration;

[Collection("Integration Tests")]
public class WeatherControllerIntegrationTests
{
    private readonly WeatherForecastWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public WeatherControllerIntegrationTests(WeatherForecastWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetWeather_WithValidTokenAndExistingCity_ReturnsOkWithData()
    {
        // Arrange - register and login
        var username = $"weatheruser_{Guid.NewGuid():N}";
        var password = "Password123!";

        var registerRequest = new RegisterRequest { Username = username, Password = password };
        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.OK, regResponse.StatusCode);

        var loginRequest = new LoginRequest { Username = username, Password = password };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Result<AuthResponse>>();
        Assert.NotNull(loginResult?.Data);
        var token = loginResult!.Data!.Token;

        var authenticatedClient = _factory.CreateClient();
        authenticatedClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await authenticatedClient.GetAsync("/api/weather?city=Cairo");

        // Assert HTTP
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<WeatherResponse>>();

        // Assert Body
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(StatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal("Cairo", result.Data.City);
        Assert.Equal("Weather data retrieved successfully", result.Message);
    }

    [Fact]
    public async Task GetWeather_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/weather?city=Cairo");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetWeather_WithEmptyCity_ReturnsBadRequest()
    {
        // Arrange - register and login
        var username = $"weatheruser_{Guid.NewGuid():N}";
        var password = "Password123!";

        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest { Username = username, Password = password });
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Username = username, Password = password });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Result<AuthResponse>>();
        var token = loginResult!.Data!.Token;

        var authenticatedClient = _factory.CreateClient();
        authenticatedClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act - omit city query
        var response = await authenticatedClient.GetAsync("/api/weather");

        // Assert HTTP
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<WeatherResponse>>();
        Assert.NotNull(result);
        Assert.Null(result.Data);
        Assert.False(result.Success);
        Assert.Equal(StatusCode.BadRequest, result.StatusCode);
        Assert.Equal("City name is required", result.Message);
    }

    [Fact]
    public async Task GetWeather_WithUnknownCity_ReturnsNotFound()
    {
        // Arrange - register and login
        var username = $"weatheruser_{Guid.NewGuid():N}";
        var password = "Password123!";

        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest { Username = username, Password = password });
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Username = username, Password = password });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Result<AuthResponse>>();
        var token = loginResult!.Data!.Token;

        var authenticatedClient = _factory.CreateClient();
        authenticatedClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Act
        var response = await authenticatedClient.GetAsync("/api/weather?city=NonExistentCity");

        // Assert HTTP
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<WeatherResponse>>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(StatusCode.NotFound, result.StatusCode);
        Assert.Equal("Weather data not found for the specified city", result.Message);
    }
}
