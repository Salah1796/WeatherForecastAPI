using WeatherForecast.Application.Interfaces;

namespace WeatherForecast.Infrastructure.Services;

/// <summary>
/// BCrypt implementation of password hashing service.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Hashes a password using BCrypt.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The hashed password.</returns>
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    /// <summary>
    /// Verifies a password against a hash.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <param name="hashedPassword">The hashed password to compare against.</param>
    /// <returns>True if the password matches, false otherwise.</returns>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}

