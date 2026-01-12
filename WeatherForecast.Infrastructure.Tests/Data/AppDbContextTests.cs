using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WeatherForecast.Domain.Entities;
using WeatherForecast.Infrastructure.Data;
using Xunit;

namespace WeatherForecast.Infrastructure.Tests.Data;

public class AppDbContextTests
{
    private readonly DbContextOptions<AppDbContext> _options;

    public AppDbContextTests()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task SaveChangesAsync_WhenAddingEntity_ShouldSetCreatedAt()
    {
        // Arrange
        using var context = new AppDbContext(_options);
        var user = new User("testuser", "hash");

        // Act
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Assert
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_WhenModifyingEntity_ShouldSetUpdatedAt()
    {
        // Arrange
        using var context = new AppDbContext(_options);
        var user = new User("testuser", "hash");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        user.IncrementFailedAttempts(5, TimeSpan.FromMinutes(15));
        context.Users.Update(user);
        await context.SaveChangesAsync();

        // Assert
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
