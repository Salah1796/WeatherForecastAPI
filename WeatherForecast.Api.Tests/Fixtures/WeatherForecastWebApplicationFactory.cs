using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using WeatherForecast.Infrastructure.Data;

namespace WeatherForecast.Api.Tests.Fixtures;

public class WeatherForecastWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<AppDbContext>();
            
            // Delete and recreate the database for a clean test environment
            context.Database.EnsureDeleted();
            context.Database.Migrate();
        }

        return host;
    }
}
