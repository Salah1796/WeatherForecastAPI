using Microsoft.Extensions.Localization;
using WeatherForecast.Application.Common.Localization;

namespace WeatherForecast.Api.Localization
{
    public class AppLocalizer(IStringLocalizer<SharedResource> localizer) : IAppLocalizer
    {
        private readonly IStringLocalizer<SharedResource> _localizer = localizer;

        public string this[string key] => _localizer[key];
    }
}