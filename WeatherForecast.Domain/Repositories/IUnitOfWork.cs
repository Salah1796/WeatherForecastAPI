namespace WeatherForecast.Domain.Repositories;

/// <summary>
/// Unit of Work interface to manage repositories and transactions.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the user repository.
    /// </summary>
    IUserRepository UserRepository { get; }

    /// <summary>
    /// Persists all changes to the data store.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync();
}
