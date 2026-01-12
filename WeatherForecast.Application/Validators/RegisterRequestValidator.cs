using FluentValidation;
using WeatherForecast.Application.Common.Localization;
using WeatherForecast.Application.DTOs;

namespace WeatherForecast.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator(IAppLocalizer _localizer)
    {
        RuleFor(x => x.Username)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(_localizer["UsernameRequired"])
            .MinimumLength(3).WithMessage(_localizer["UsernameTooShort"]);

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(_localizer["PasswordRequired"])
            .MinimumLength(8).WithMessage(_localizer["PasswordTooWeak"])
            .Matches(@"[A-Z]").WithMessage(_localizer["PasswordTooWeak"])
            .Matches(@"[a-z]").WithMessage(_localizer["PasswordTooWeak"])
            .Matches(@"[0-9]").WithMessage(_localizer["PasswordTooWeak"])
            .Matches(@"[\!\?\*\.]").WithMessage(_localizer["PasswordTooWeak"]);
    }
}

