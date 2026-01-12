using FluentAssertions;
using Moq;
using WeatherForecast.Application.Common.Localization;
using WeatherForecast.Application.DTOs;
using WeatherForecast.Application.Validators;
using Xunit;

namespace WeatherForecast.Application.Tests.Validators;

public class RegisterRequestValidatorTests
{
    private readonly Mock<IAppLocalizer> _mockLocalizer;
    private readonly RegisterRequestValidator _validator;

    public RegisterRequestValidatorTests()
    {
        _mockLocalizer = new Mock<IAppLocalizer>();
        _mockLocalizer.Setup(l => l["UsernameRequired"]).Returns("Username is Required");
        _mockLocalizer.Setup(l => l["UsernameTooShort"]).Returns("Username must be at least 3 characters long");
        _mockLocalizer.Setup(l => l["PasswordRequired"]).Returns("Password is required");
        _mockLocalizer.Setup(l => l["PasswordTooWeak"]).Returns("Password too weak");
        
        _validator = new RegisterRequestValidator(_mockLocalizer.Object);
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldPass()
    {
        var request = new RegisterRequest { Username = "ValidUser", Password = "StrongPassword1!" };
        var result = _validator.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithShortUsername_ShouldFail()
    {
        var request = new RegisterRequest { Username = "ab", Password = "StrongPassword1!" };
        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Username must be at least 3 characters long");
    }

    [Theory]
    [InlineData("short1!", "Password too weak")] // Too short
    [InlineData("NoDigit!!", "Password too weak")] // No digit
    [InlineData("noup!!1", "Password too weak")] // No upper
    [InlineData("NOLOWER1!", "Password too weak")] // No lower
    [InlineData("NoSpecial1", "Password too weak")] // No special
    public void Validate_WithWeakPassword_ShouldFail(string password, string expectedError)
    {
        var request = new RegisterRequest { Username = "ValidUser", Password = password };
        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == expectedError);
    }
}
