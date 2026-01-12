using WeatherForecast.Application.Common.Results;
using WeatherForecast.Application.DTOs;

namespace WeatherForecast.Application.Interfaces;

/// <summary>
/// Interface for authentication service operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="request">The registration request containing username and password.</param>
    /// <returns>A result containing the authentication response with JWT token if successful, or error details if failed.</returns>
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Authenticates a user and generates a JWT token.
    /// </summary>
    /// <param name="request">The login request containing username and password.</param>
    /// <returns>A result containing the authentication response with JWT token if successful, or error details if failed.</returns>
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
}

