using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Application.Services;
using WeatherForecast.Application.Validators;

namespace WeatherForecast.Application;

/// <summary>
/// Extension methods for dependency injection configuration.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds application services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register FluentValidation validators
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        // Register application services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IWeatherService, WeatherService>();

        return services;
    }
}

