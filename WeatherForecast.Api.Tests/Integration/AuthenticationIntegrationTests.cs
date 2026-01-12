using System.Net;
using WeatherForecast.Api.Tests.Fixtures;
using WeatherForecast.Application.Common.Enums;
using WeatherForecast.Application.Common.Results;
using WeatherForecast.Application.DTOs;
using Xunit;

namespace WeatherForecast.Api.Tests.Integration;

[Collection("Integration Tests")]
public class AuthenticationIntegrationTests
{
    private readonly HttpClient _client;
    public AuthenticationIntegrationTests(WeatherForecastWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    #region Registration Tests

    [Fact]
    public async Task Register_WithValidRequest_ReturnsOkWithToken()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Username = "testuser",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert HTTP
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<AuthResponse>>();

        // Assert Body
        Assert.NotNull(result);
        Assert.Equal(StatusCode.OK, result.StatusCode);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("testuser", result.Data.Username);
        Assert.NotEmpty(result.Data.Token);
        Assert.NotEqual(result.Data.UserId, Guid.Empty);
        Assert.Equal("User registered successfully", result.Message);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task Register_WithEmptyUsername_ReturnsBadRequest()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Username = "",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert HTTP
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<AuthResponse>>();

        // Assert Body
        Assert.NotNull(result);
        Assert.Equal(StatusCode.BadRequest, result.StatusCode);
        Assert.False(result.Success);
        Assert.Equal("Validation failed",result.Message);
        Assert.Single(result.Errors);
        Assert.Equal("Username is Required", result.Errors.First());
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task Register_WithEmptyPassword_ReturnsBadRequest()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Username = "testuser",
            Password = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert HTTP
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<AuthResponse>>();

        // Assert Body
        Assert.NotNull(result);
        Assert.Equal(StatusCode.BadRequest, result.StatusCode);
        Assert.False(result.Success);
        Assert.Equal("Validation failed", result.Message);
        Assert.Single(result.Errors);
        Assert.Equal("Password is required", result.Errors.First());
        Assert.Null(result.Data);
    }


    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsConflict()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Username = "duplicateuser",
            Password = "Password123!"
        };

        // Register the user first
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act - Try to register with same username
        response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert

        // Assert HTTP
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<AuthResponse>>();

        // Assert Body
        Assert.NotNull(result);
        Assert.Equal(StatusCode.Conflict, result.StatusCode);
        Assert.False(result.Success);
        Assert.Equal("Username already exists", result.Message);
        Assert.Empty(result.Errors);
        Assert.Null(result.Data);
    }


    [Fact]
    public async Task Register_MultipleUsers_ReturnsUniqueTokens()
    {
        // Arrange
        var user1 = new RegisterRequest { Username = "user1", Password = "Password123!" };
        var user2 = new RegisterRequest { Username = "user2", Password = "Password456!" };

        // Act
        var response1 = await _client.PostAsJsonAsync("/api/auth/register", user1);

        var response2 = await _client.PostAsJsonAsync("/api/auth/register", user2);


        // Assert

        // Assert HTTP
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);


        // Assert Body
        var result1 = await response1.Content.ReadFromJsonAsync<Result<AuthResponse>>();
        var result2 = await response2.Content.ReadFromJsonAsync<Result<AuthResponse>>();

        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(StatusCode.OK, result1.StatusCode);
        Assert.Equal(StatusCode.OK, result2.StatusCode);
        Assert.True(result1.Success);
        Assert.True(result2.Success);
        Assert.Empty(result1.Errors);
        Assert.Empty(result2.Errors);
        Assert.NotNull(result1.Data);
        Assert.NotNull(result2.Data);
        Assert.NotEqual(result1.Data.Token, result2.Data.Token);

    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange - register first
        var registerRequest = new RegisterRequest { Username = "loginuser", Password = "Password123!" };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var loginRequest = new LoginRequest { Username = "loginuser", Password = "Password123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert HTTP
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<AuthResponse>>();

        // Assert Body
        Assert.NotNull(result);
        Assert.Equal(StatusCode.OK, result.StatusCode);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("loginuser", result.Data.Username);
        Assert.NotEmpty(result.Data.Token);
        Assert.NotEqual(result.Data.UserId, Guid.Empty);
        Assert.Equal("Login successful", result.Message);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task Login_WithEmptyUsername_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest { Username = "", Password = "Password123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert HTTP
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<AuthResponse>>();

        // Assert Body
        Assert.NotNull(result);
        Assert.Equal(StatusCode.BadRequest, result.StatusCode);
        Assert.False(result.Success);
        Assert.Equal("Validation failed", result.Message);
        Assert.Single(result.Errors);
        Assert.Equal("Username is Required", result.Errors.First());
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest { Username = "loginuser", Password = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert HTTP
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<AuthResponse>>();

        // Assert Body
        Assert.NotNull(result);
        Assert.Equal(StatusCode.BadRequest, result.StatusCode);
        Assert.False(result.Success);
        Assert.Equal("Validation failed", result.Message);
        Assert.Single(result.Errors);
        Assert.Equal("Password is required", result.Errors.First());
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange - register user
        var registerRequest = new RegisterRequest { Username = "invaliduser", Password = "Password123!" };
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var loginRequest = new LoginRequest { Username = "invaliduser", Password = "WrongPassword" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert HTTP
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<AuthResponse>>();

        // Assert Body
        Assert.NotNull(result);
        Assert.Equal(StatusCode.Unauthorized, result.StatusCode);
        Assert.False(result.Success);
        Assert.Equal("Invalid username or password", result.Message);
        Assert.Empty(result.Errors);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest { Username = "nouser", Password = "Password123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert HTTP
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<AuthResponse>>();

        // Assert Body
        Assert.NotNull(result);
        Assert.Equal(StatusCode.Unauthorized, result.StatusCode);
        Assert.False(result.Success);
        Assert.Equal("Invalid username or password", result.Message);
        Assert.Empty(result.Errors);
        Assert.Null(result.Data);
    }

    #endregion

}
