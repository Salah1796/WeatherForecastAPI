using FluentValidation;
using Microsoft.Extensions.Options;
using WeatherForecast.Application.Common.Enums;
using WeatherForecast.Application.Common.Localization;
using WeatherForecast.Application.Common.Options;
using WeatherForecast.Application.Common.Results;
using WeatherForecast.Application.DTOs;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Domain.Entities;
using WeatherForecast.Domain.Repositories;

namespace WeatherForecast.Application.Services;

/// <summary>
/// Service for handling user authentication operations including registration and login.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IAppLocalizer _localizer;
    private readonly SecuritySettings _securitySettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for database operations.</param>
    /// <param name="passwordHasher">The password hasher for secure password operations.</param>
    /// <param name="tokenGenerator">The token generator for JWT token creation.</param>
    /// <param name="registerValidator">The validator for registration requests.</param>
    /// <param name="loginValidator">The validator for login requests.</param>
    /// <param name="localizer">The localizer for retrieving localized messages.</param>
    /// <param name="securitySettings">The security settings configuration.</param>
    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenGenerator tokenGenerator,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
       IAppLocalizer localizer,
       IOptions<SecuritySettings> securitySettings)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _localizer = localizer;
        _securitySettings = securitySettings.Value;
    }

    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="request">The registration request containing username and password.</param>
    /// <returns>A result containing the authentication response with JWT token if successful, or error details if failed.</returns>
    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
    {

        var validationResult = await _registerValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Result<AuthResponse>.ValidationError(validationResult, _localizer["ValidationFailed"]);
        }

        var userExists = await _unitOfWork.UserRepository.UserExistsAsync(request.Username);
        if (userExists)
            return Result<AuthResponse>.ErrorResponse(_localizer["UsernameAlreadyExists"], StatusCode.Conflict);

        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var user = new User(request.Username, passwordHash);
        
         _unitOfWork.UserRepository.Add(user);
        await _unitOfWork.SaveChangesAsync();

        var token = _tokenGenerator.GenerateToken(user);

        var authResponse = new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Username = user.Username
        };

        return Result<AuthResponse>.SuccessResponse(authResponse, _localizer["UserRegisteredSuccessfully"]);
    }

    /// <summary>
    /// Authenticates a user and generates a JWT token.
    /// </summary>
    /// <param name="request">The login request containing username and password.</param>
    /// <returns>A result containing the authentication response with JWT token if successful, or error details if failed.</returns>
    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var validationResult = await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return Result<AuthResponse>.ValidationError(validationResult, _localizer["ValidationFailed"]);

        var user = await _unitOfWork.UserRepository.GetByUsernameAsync(request.Username);
        if (user == null)
            return Result<AuthResponse>.ErrorResponse(_localizer["InvalidCredentials"], StatusCode.Unauthorized);

        // Check if account is locked
        if (user.IsLockedOut())
        {
            var lockoutTimeRemaining = user.LockoutEnd!.Value.Subtract(DateTime.UtcNow);
            var message = string.Format(_localizer["AccountLocked"], Math.Ceiling(lockoutTimeRemaining.TotalMinutes));
            return Result<AuthResponse>.ErrorResponse(message, StatusCode.Unauthorized);
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.IncrementFailedAttempts(
                _securitySettings.MaxFailedLoginAttempts,
                TimeSpan.FromMinutes(_securitySettings.AccountLockoutDurationMinutes));
            
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();
            
            return Result<AuthResponse>.ErrorResponse(_localizer["InvalidCredentials"], StatusCode.Unauthorized);
        }

        // Successful login - reset failed attempts
        user.ResetFailedAttempts();
        _unitOfWork.UserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        var token = _tokenGenerator.GenerateToken(user);

        var authResponse = new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Username = user.Username
        };

        return Result<AuthResponse>.SuccessResponse(authResponse, _localizer["LoginSuccessful"]);
    }
}

