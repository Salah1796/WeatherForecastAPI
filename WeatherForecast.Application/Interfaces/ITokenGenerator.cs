using WeatherForecast.Domain.Entities;

namespace WeatherForecast.Application.Interfaces;

public interface ITokenGenerator
{
    string GenerateToken(User user);
}

