using WeatherForecast.Domain.Entities;
using WeatherForecast.Domain.Repositories;
using WeatherForecast.Infrastructure.Data;

namespace WeatherForecast.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of the user repository.
/// </summary>
/// <param name="context"></param>
public class EfUserRepository(AppDbContext context) : GenericRepository<User>(context), IUserRepository
{

    /// <summary>
    /// Gets a user by their username.
    /// </summary>
    /// <param name="username">The username of the user to get.</param>
    /// <returns>The user with the specified username or null if not found.</returns>
    public Task<User?> GetByUsernameAsync(string username)
    {
        return FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    /// <summary>
    /// Checks if a user exists by their username.
    /// </summary>
    /// <param name="username">The username of the user to check.</param>
    /// <returns>True if the user exists, false otherwise.</returns>
    public Task<bool> UserExistsAsync(string username)
    {
        return AnyAsync(u => u.Username.ToLower() == username.ToLower());
    }
}
