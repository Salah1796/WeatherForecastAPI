using WeatherForecast.Domain.Entities;
using WeatherForecast.Domain.Repositories;

namespace WeatherForecast.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of the user repository.
/// </summary>
public class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<string, User> _users = [];
    private readonly object _lock = new();

    /// <summary>
    /// Gets a user by their username.
    /// </summary>
    /// <param name="username">The username of the user to get.</param>
    /// <returns>The user with the specified username.</returns>
    public Task<User?> GetByUsernameAsync(string username)
    {
        lock (_lock)
        {
            _users.TryGetValue(username.ToLowerInvariant(), out var user);
            return Task.FromResult(user);
        }
    }

    /// <summary>
    /// Checks if a user exists by their username.
    /// </summary>
    /// <param name="username">The username of the user to check.</param>
    /// <returns>True if the user exists, false otherwise.</returns>
    public Task<bool> UserExistsAsync(string username)
    {
        lock (_lock)
        {
            return Task.FromResult(_users.ContainsKey(username.ToLowerInvariant()));
        }
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="user">The user to create.</param>
    /// <returns>The created user.</returns>
    public void Add(User user)
    {
        lock (_lock)
        {
            var key = user.Username.ToLowerInvariant();
            if (_users.ContainsKey(key))
            {
                throw new InvalidOperationException($"User with username '{user.Username}' already exists.");
            }
            _users[key] = user;
        }
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <returns>The updated user.</returns>
    public void Update(User user)
    {
        lock (_lock)
        {
            var key = user.Username.ToLowerInvariant();
            if (!_users.ContainsKey(key))
            {
                throw new InvalidOperationException($"User with username '{user.Username}' does not exist.");
            }
            _users[key] = user;
        }
    }

}

