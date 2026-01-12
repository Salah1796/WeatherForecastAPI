using System.Net;
using WeatherForecast.Api.Tests.Fixtures;
using WeatherForecast.Application.Common.Results;
using WeatherForecast.Application.DTOs;
using Xunit;

namespace WeatherForecast.Api.Tests.Integration;

[Collection("Integration Tests")]
public class RateLimitingIntegrationTests
{
    private readonly WeatherForecastWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RateLimitingIntegrationTests(WeatherForecastWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetWeather_ExceedRateLimit_ReturnsTooManyRequests()
    {
        // 1. Arrange: Authenticate to get a valid token
        var username = $"ratelimituser_{Guid.NewGuid():N}";
        var token = await RegisterAndLogin(username);

        var authenticatedClient = _factory.CreateClient();
        authenticatedClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // 2. Act: Send 100 allowed requests
        for (int i = 0; i < 100; i++)
        {
            var response = await authenticatedClient.GetAsync("/api/weather?city=Cairo");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // 3. Act: Send the 101th request
        var blockedResponse = await authenticatedClient.GetAsync("/api/weather?city=Cairo");

        // 4. Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, blockedResponse.StatusCode);
    }

    [Fact]
    public async Task RateLimit_IsPartitionedByUser()
    {
        // 1. User A exceeds limit
        var userA = $"userA_{Guid.NewGuid():N}";
        var tokenA = await RegisterAndLogin(userA);
        var clientA = _factory.CreateClient();
        clientA.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenA}");

        for (int i = 0; i < 100; i++)
        {
            await clientA.GetAsync("/api/weather?city=Cairo");
        }
        var responseA = await clientA.GetAsync("/api/weather?city=Cairo");
        Assert.Equal(HttpStatusCode.TooManyRequests, responseA.StatusCode);

        // 2. User B should still be allowed (different partition)
        var userB = $"userB_{Guid.NewGuid():N}";
        var tokenB = await RegisterAndLogin(userB);
        var clientB = _factory.CreateClient();
        clientB.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenB}");

        var responseB = await clientB.GetAsync("/api/weather?city=Cairo");
        Assert.Equal(HttpStatusCode.OK, responseB.StatusCode);
    }

    private async Task<string> RegisterAndLogin(string username)
    {
        var password = "Password123!";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest { Username = username, Password = password });
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Username = username, Password = password });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Result<AuthResponse>>();
        return loginResult!.Data!.Token;
    }
}
