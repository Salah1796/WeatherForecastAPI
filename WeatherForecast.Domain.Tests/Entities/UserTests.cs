using FluentAssertions;
using WeatherForecast.Domain.Entities;

namespace WeatherForecast.Domain.Tests.Entities;

public class UserTests
{
   
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var username = "testuser";
        var passwordHash = "hashedpassword123";

        // Act
        var user = new User(username, passwordHash);

        // Assert
        user.Should().NotBeNull();
        user.Username.Should().Be(username);
        user.PasswordHash.Should().Be(passwordHash);
        user.Id.Should().NotBeEmpty();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldGenerateUniqueId()
    {
        // Arrange
        var username1 = "user1";
        var username2 = "user2";
        var passwordHash = "hashedpassword123";

        // Act
        var user1 = new User(username1, passwordHash);
        var user2 = new User(username2, passwordHash);

        // Assert
        user1.Id.Should().NotBe(user2.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WithNullEmptyOrWhitespaceUsername_ShouldThrowArgumentNullException(string? username)
    {
        // Arrange
        var passwordHash = "hashedpassword123";

        // Act
        var act = () => new User(username!, passwordHash);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("username")
            .WithMessage("*Username cannot be empty.*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WithNullEmptyOrWhitespacePasswordHash_ShouldThrowArgumentNullException(string? passwordHash)
    {
        // Arrange
        var username = "testuser";

        // Act
        var act = () => new User(username, passwordHash!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("passwordHash")
            .WithMessage("*Password hash cannot be empty.*");
    }


    [Fact]
    public void User_ShouldInheritFromAuditableEntity()
    {
        // Arrange
        var username = "testuser";
        var passwordHash = "hashedpassword123";

        // Act
        var user = new User(username, passwordHash);

        // Assert
        user.Should().BeAssignableTo<Domain.Common.AuditableEntity>();
    }

    [Fact]
    public void IsLockedOut_WhenLockoutEndInPast_ShouldReturnFalse()
    {
        // Arrange
        var user = new User("testuser", "hash");
        user.IncrementFailedAttempts(1, TimeSpan.FromMinutes(-15));

        // Act
        var isLocked = user.IsLockedOut();

        // Assert
        isLocked.Should().BeFalse();
    }

    [Fact]
    public void IsLockedOut_WhenLockoutEndInFuture_ShouldReturnTrue()
    {
        // Arrange
        var user = new User("testuser", "hash");
        user.IncrementFailedAttempts(1, TimeSpan.FromMinutes(15));

        // Act
        var isLocked = user.IsLockedOut();

        // Assert
        isLocked.Should().BeTrue();
    }

    [Fact]
    public void IncrementFailedAttempts_BelowThreshold_ShouldNotLockout()
    {
        // Arrange
        var user = new User("testuser", "hash");

        // Act
        user.IncrementFailedAttempts(5, TimeSpan.FromMinutes(15));

        // Assert
        user.FailedLoginAttempts.Should().Be(1);
        user.IsLockedOut().Should().BeFalse();
        user.LockoutEnd.Should().BeNull();
    }

    [Fact]
    public void IncrementFailedAttempts_AtThreshold_ShouldLockout()
    {
        // Arrange
        var user = new User("testuser", "hash");

        // Act
        for(int i=0; i<5; i++)
            user.IncrementFailedAttempts(5, TimeSpan.FromMinutes(15));

        // Assert
        user.FailedLoginAttempts.Should().Be(5);
        user.IsLockedOut().Should().BeTrue();
        user.LockoutEnd.Should().NotBeNull();
    }

    [Fact]
    public void ResetFailedAttempts_ShouldClearCounters()
    {
        // Arrange
        var user = new User("testuser", "hash");
        user.IncrementFailedAttempts(1, TimeSpan.FromMinutes(15));

        // Act
        user.ResetFailedAttempts();

        // Assert
        user.FailedLoginAttempts.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
        user.IsLockedOut().Should().BeFalse();
    }
}

