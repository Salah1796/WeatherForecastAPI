using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WeatherForecast.Infrastructure.Data;

namespace WeatherForecast.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation using EF Core.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="GenericRepository{T}"/> class.
/// </remarks>
/// <param name="context">The database context.</param>
public class GenericRepository<T>(AppDbContext context) where T : class
{
    protected readonly AppDbContext _context = context;


    /// <summary>
    /// Checks if any entity matches the predicate.
    /// </summary>
    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return _context.Set<T>().AnyAsync(predicate);
    }

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return _context.Set<T>().FirstOrDefaultAsync(predicate);
    }

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    public void Add(T entity)
    {
        _context.Set<T>().Add(entity);
    }

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    public void Update(T entity)
    {
        _context.Set<T>().Update(entity);
    }
}
