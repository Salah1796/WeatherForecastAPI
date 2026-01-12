namespace WeatherForecast.Application.DTOs;

public class WeatherResponse
{
    public string City { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public string Condition { get; set; } = string.Empty;
}

