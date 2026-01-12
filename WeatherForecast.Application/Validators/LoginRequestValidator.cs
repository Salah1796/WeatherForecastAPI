using FluentValidation;
using WeatherForecast.Application.Common.Localization;
using WeatherForecast.Application.DTOs;

namespace WeatherForecast.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator(IAppLocalizer _localizer)
    {
        RuleFor(x => x.Username)
           .NotEmpty().WithMessage(_localizer["UsernameRequired"]);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(_localizer["PasswordRequired"]);
    }
}

