namespace WeatherForecast.Domain.ValueObjects;
/// <summary>
/// Represents a weather forecast for a specific city.
/// </summary>
public class WeatherForecast
{
    /// <summary>
    /// Creates a new weather forecast for a specific city.
    /// </summary>
    /// <param name="city">The city for the weather forecast.</param>
    /// <param name="temperature">The temperature for the weather forecast.</param>
    /// <param name="condition">The condition for the weather forecast.</param>
    /// <exception cref="ArgumentNullException">Thrown when city is null or empty.</exception>
    public WeatherForecast(
        string city,
        double temperature,
        string condition)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentNullException(nameof(city));
        City = city;
        Temperature = temperature;
        Condition = condition;
    }

     /// <summary>
    /// The city for the weather forecast.
    /// </summary>
    public string City { get; }
    
    /// <summary>
    /// The temperature for the weather forecast.
    /// </summary>
    public double Temperature { get; }
    
    /// <summary>
    /// The condition for the weather forecast.
    /// </summary>
    public string Condition { get; }
}
