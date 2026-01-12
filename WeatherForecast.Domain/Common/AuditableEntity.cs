namespace WeatherForecast.Domain.Common;

/// <summary>
/// Base class for entities that need auditing.
/// </summary>
public class AuditableEntity : IAuditableEntity
{
    /// <summary>
    /// The date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The date and time when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}