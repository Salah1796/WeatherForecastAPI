namespace WeatherForecast.Application.Common.Options;

/// <summary>
/// Security settings for authentication and authorization.
/// </summary>
public class SecuritySettings
{
    /// <summary>
    /// Maximum number of failed login attempts before account lockout.
    /// </summary>
    public int MaxFailedLoginAttempts { get; set; } = 5;

    /// <summary>
    /// Duration of account lockout in minutes.
    /// </summary>
    public int AccountLockoutDurationMinutes { get; set; } = 15;

    /// <summary>
    /// Rate limit for authentication endpoints (requests per minute).
    /// </summary>
    public int AuthRateLimitPerMinute { get; set; } = 5;

    /// <summary>
    /// Rate limit for weather endpoints (requests per minute).
    /// </summary>
    public int WeatherRateLimitPerMinute { get; set; } = 10;
}
