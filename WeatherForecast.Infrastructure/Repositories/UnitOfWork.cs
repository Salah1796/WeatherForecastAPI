using WeatherForecast.Domain.Repositories;
using WeatherForecast.Infrastructure.Data;

namespace WeatherForecast.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UnitOfWork(AppDbContext context, IUserRepository userRepository)
    {
        _context = context;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Gets the user repository.
    /// </summary>
    public IUserRepository UserRepository => _userRepository;

    /// <summary>
    /// Persists all changes to the database.
    /// </summary>
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Disposes the database context.
    /// </summary>
    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
