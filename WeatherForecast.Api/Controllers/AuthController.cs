using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WeatherForecast.Application.Common.Results;
using WeatherForecast.Application.DTOs;
using WeatherForecast.Application.Interfaces;

namespace WeatherForecast.Api.Controllers;

/// <summary>
/// Authentication controller for user registration and login.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="authService">The authentication service.</param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="request">The registration request containing username and password.</param>
    /// <returns>An authentication response with JWT token if successful.</returns>
    /// <response code="200">User registered successfully with JWT token.</response>
    /// <response code="400">Validation failed or invalid request.</response>
    /// <response code="409">Username already exists.</response>
    [HttpPost("register")]
    [ProducesResponseType<Result<AuthResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<AuthResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<AuthResponse>>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {

        var result = await _authService.RegisterAsync(request);

        return StatusCode((int)result.StatusCode, result);
    }

    /// <summary>
    /// Authenticates a user and generates a JWT token.
    /// </summary>
    /// <param name="request">The login request containing username and password.</param>
    /// <returns>An authentication response with JWT token if successful.</returns>
    /// <response code="200">User logged in successfully with JWT token.</response>
    /// <response code="400">Validation failed or invalid request.</response>
    /// <response code="401">Invalid credentials.</response>
    [HttpPost("login")]
    [ProducesResponseType<Result<AuthResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<AuthResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<AuthResponse>>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        return StatusCode((int)result.StatusCode, result);
    }
}
