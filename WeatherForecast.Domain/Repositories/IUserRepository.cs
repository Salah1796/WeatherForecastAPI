using WeatherForecast.Domain.Entities;

namespace WeatherForecast.Domain.Repositories;

/// <summary>
/// Interface for the user repository.
/// </summary>
public interface IUserRepository
{
   
    /// <summary>
    /// Gets a user by their username.
    /// </summary>
    /// <param name="username">The username of the user to get.</param>
    /// <returns>The user with the specified username.</returns>
    Task<User?> GetByUsernameAsync(string username);
    
    /// <summary>
    /// Checks if a user exists by their username.
    /// </summary>
    /// <param name="username">The username of the user to check.</param>
    /// <returns>True if the user exists, false otherwise.</returns>
    Task<bool> UserExistsAsync(string username);

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="user">The user to create.</param>
    void Add(User user);
    
    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="user">The user to update.</param>
    void Update(User user);
}

