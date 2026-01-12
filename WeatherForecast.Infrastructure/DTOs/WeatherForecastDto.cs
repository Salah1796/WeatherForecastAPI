namespace WeatherForecast.Infrastructure.DTOs;

public class WeatherForecastDto
{
    public string City { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public string Condition { get; set; } = string.Empty;
  
}

