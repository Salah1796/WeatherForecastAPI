using FluentValidation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using WeatherForecast.Application.Common.Enums;
using WeatherForecast.Application.Common.Localization;
using WeatherForecast.Application.Common.Options;
using WeatherForecast.Application.DTOs;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Application.Services;
using WeatherForecast.Domain.Entities;
using WeatherForecast.Domain.Repositories;
using Xunit;

namespace WeatherForecast.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly Mock<IValidator<RegisterRequest>> _registerValidatorMock;
    private readonly Mock<IValidator<LoginRequest>> _loginValidatorMock;
    private readonly Mock<IAppLocalizer> _localizerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenGeneratorMock = new Mock<ITokenGenerator>();
        _registerValidatorMock = new Mock<IValidator<RegisterRequest>>();
        _loginValidatorMock = new Mock<IValidator<LoginRequest>>();
        _localizerMock = new Mock<IAppLocalizer>();

        _unitOfWorkMock.Setup(x => x.UserRepository).Returns(_userRepositoryMock.Object);

        // Setup localizer to return the key as the value
        _localizerMock.Setup(x => x[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        // Setup SecuritySettings
        var securitySettings = new SecuritySettings
        {
            MaxFailedLoginAttempts = 5,
            AccountLockoutDurationMinutes = 15
        };
        var securityOptionsMock = new Mock<IOptions<SecuritySettings>>();
        securityOptionsMock.Setup(x => x.Value).Returns(securitySettings);

        _authService = new AuthService(
            _unitOfWorkMock.Object,
            _passwordHasherMock.Object,
            _tokenGeneratorMock.Object,
            _registerValidatorMock.Object,
            _loginValidatorMock.Object,
            _localizerMock.Object,
            securityOptionsMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var request = new RegisterRequest { Username = "testuser", Password = "Password123!" };
        var hashedPassword = "hashed_password";
        var validationResult = new FluentValidation.Results.ValidationResult();
        var user = new User(request.Username, hashedPassword);
        const string token = "jwt_token";

        _registerValidatorMock.Setup(x => x.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(validationResult);
        _userRepositoryMock.Setup(x => x.UserExistsAsync(request.Username))
            .ReturnsAsync(false);
        _passwordHasherMock.Setup(x => x.HashPassword(request.Password))
            .Returns(hashedPassword);
        _userRepositoryMock.Setup(x => x.Add(It.IsAny<User>()));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        _tokenGeneratorMock.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(token);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(request.Username, result.Data.Username);
        Assert.Equal(token, result.Data.Token);
        Assert.Equal(StatusCode.OK, result.StatusCode);
        Assert.Equal("UserRegisteredSuccessfully", result.Message);
        
        _userRepositoryMock.Verify(x => x.UserExistsAsync(request.Username), Times.Once);
        _userRepositoryMock.Verify(x => x.Add(It.Is<User>(u => u.Username == request.Username)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(It.Is<User>(u => u.Username == request.Username)), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ReturnsConflictError()
    {
        // Arrange
        var request = new RegisterRequest { Username = "existinguser", Password = "Password123!" };
        var validationResult = new FluentValidation.Results.ValidationResult();

        _registerValidatorMock.Setup(x => x.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(validationResult);
        _userRepositoryMock.Setup(x => x.UserExistsAsync(request.Username))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(StatusCode.Conflict, result.StatusCode);
        Assert.Equal("UsernameAlreadyExists", result.Message);
        _userRepositoryMock.Verify(x => x.UserExistsAsync(request.Username), Times.Once);
        _userRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidUsername_ReturnsValidationError()
    {
        // Arrange
        var request = new RegisterRequest { Username = "", Password = "password" };
        var validationFailure = new FluentValidation.Results.ValidationFailure("Username", "Username is required");
        var validationResult = new FluentValidation.Results.ValidationResult([validationFailure]);

        _registerValidatorMock.Setup(x => x.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(StatusCode.BadRequest, result.StatusCode);
        Assert.Equal("ValidationFailed", result.Message);
        _userRepositoryMock.Verify(x => x.Add(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccessResult()
    {
        // Arrange
        var request = new LoginRequest { Username = "testuser", Password = "Password123!" };
        var hashedPassword = "hashed_password";
        var validationResult = new FluentValidation.Results.ValidationResult();
        var user = new User(request.Username, hashedPassword);
        const string token = "jwt_token";

        _loginValidatorMock.Setup(x => x.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(validationResult);
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(request.Username))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);
        _userRepositoryMock.Setup(x => x.Update(It.IsAny<User>()));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        _tokenGeneratorMock.Setup(x => x.GenerateToken(user))
            .Returns(token);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(user.Username, result.Data.Username);
        Assert.Equal(token, result.Data.Token);
        Assert.Equal(StatusCode.OK, result.StatusCode);
        Assert.Equal("LoginSuccessful", result.Message);
        
        _userRepositoryMock.Verify(x => x.GetByUsernameAsync(request.Username), Times.Once);
        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(user), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidRequest_ReturnsValidationError()
    {
        // Arrange
        var request = new LoginRequest { Username = "", Password = "" };
        var validationFailure = new FluentValidation.Results.ValidationFailure("Username", "Username is required");
        var validationResult = new FluentValidation.Results.ValidationResult([validationFailure]);

        _loginValidatorMock.Setup(x => x.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(StatusCode.BadRequest, result.StatusCode);
        _userRepositoryMock.Verify(x => x.GetByUsernameAsync(request.Username), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ReturnsUnauthorizedError()
    {
        // Arrange
        var request = new LoginRequest { Username = "testuser", Password = "WrongPassword" };
        var hashedPassword = "hashed_password";
        var validationResult = new FluentValidation.Results.ValidationResult();
        var user = new User(request.Username, hashedPassword);

        _loginValidatorMock.Setup(x => x.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(validationResult);
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(request.Username))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(false);
        _userRepositoryMock.Setup(x => x.Update(It.IsAny<User>()));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(StatusCode.Unauthorized, result.StatusCode);
        Assert.Equal("InvalidCredentials", result.Message);
        
        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUser_ReturnsUnauthorizedError()
    {
        // Arrange
        var request = new LoginRequest { Username = "nonexistentuser", Password = "Password123!" };
        var validationResult = new FluentValidation.Results.ValidationResult();

        _loginValidatorMock.Setup(x => x.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(validationResult);
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(request.Username))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(StatusCode.Unauthorized, result.StatusCode);
        _userRepositoryMock.Verify(x => x.GetByUsernameAsync(request.Username), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithLockedOutUser_ReturnsUnauthorizedErrorWithLockoutMessage()
    {
        // Arrange
        var request = new LoginRequest { Username = "lockeduser", Password = "Password123!" };
        var hashedPassword = "hashed_password";
        var validationResult = new FluentValidation.Results.ValidationResult();
        
        // Create a user and manually lock it out
        var user = new User(request.Username, hashedPassword);
        
        // Lock the user
        for(int i=0; i<5; i++) user.IncrementFailedAttempts(5, TimeSpan.FromMinutes(15));
        
        _loginValidatorMock.Setup(x => x.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(validationResult);
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(request.Username))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(StatusCode.Unauthorized, result.StatusCode);
        Assert.Contains("AccountLocked", result.Message);
        
        _passwordHasherMock.Verify(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
