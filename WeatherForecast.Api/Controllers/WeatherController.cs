using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WeatherForecast.Application.Common.Localization;
using WeatherForecast.Application.Common.Results;
using WeatherForecast.Application.DTOs;
using WeatherForecast.Application.Interfaces;

namespace WeatherForecast.Api.Controllers;

/// <summary>
/// Weather controller for retrieving weather forecast data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("fixed")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly IAppLocalizer _localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeatherController"/> class.
    /// </summary>
    /// <param name="weatherService">The weather service.</param>
    public WeatherController(IWeatherService weatherService, IAppLocalizer localizer)
    {
        _weatherService = weatherService;
        _localizer = localizer;
    }


    /// <summary>
    /// Retrieves the current weather information for the specified city.
    /// </summary>
    /// <param name="city">The name of the city for which to obtain weather data. This value must not be null, empty, or consist only of
    /// whitespace.</param>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="Result{WeatherResponse}"/> with the weather information
    /// if successful; otherwise, an error response indicating the reason for failure.</returns>
    [HttpGet]
    [ProducesResponseType<Result<WeatherResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Result<WeatherResponse>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Result<WeatherResponse>>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<Result<WeatherResponse>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWeatherByCity([FromQuery] string? city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return BadRequest(Result<WeatherResponse>.ErrorResponse(_localizer["CityNameRequired"], Application.Common.Enums.StatusCode.BadRequest));
        }
        var result = await _weatherService.GetWeatherByCityAsync(city);

        return StatusCode((int)result.StatusCode, result);
    }
}
