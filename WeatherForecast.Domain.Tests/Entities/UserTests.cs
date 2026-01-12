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
}

