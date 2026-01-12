using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeatherForecast.Application.Common.Localization;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Domain.Repositories;
using WeatherForecast.Infrastructure.Data;
using WeatherForecast.Infrastructure.Repositories;
using WeatherForecast.Infrastructure.Services;

namespace WeatherForecast.Infrastructure;

/// <summary>
/// Extension methods for dependency injection configuration.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DB Context
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Register memory cache
        services.AddMemoryCache();

        // Register Redis cache
        var redisConnectionString = configuration.GetConnectionString("Redis");
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "WeatherForecast_";
        });

        // Register repositories
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddSingleton<IWeatherRepository, MockWeatherRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();

        // Register cached weather service decorator
        // We decide which caching implementation to use based on configuration
        var cacheProvider = configuration.GetValue<string>("CacheSettings:CacheProvider") ?? "Memory";

        if (cacheProvider.Equals("Redis", StringComparison.OrdinalIgnoreCase))
        {
            services.Decorate<IWeatherService>((inner, sp) =>
            {
                var cache = sp.GetRequiredService<IDistributedCache>();
                var iappLocalizer = sp.GetRequiredService<IAppLocalizer>();
                return new RedisWeatherCacheService(inner, cache, configuration, iappLocalizer);
            });
        }
        else
        {
            services.Decorate<IWeatherService>((inner, sp) =>
            {
                var cache = sp.GetRequiredService<IMemoryCache>();
                var iappLocalizer = sp.GetRequiredService<IAppLocalizer>();
                return new CachedWeatherService(inner, cache, configuration, iappLocalizer);
            });
        }

        return services;
    }
}

