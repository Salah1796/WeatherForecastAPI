using WeatherForecast.Application.Common.Enums;

namespace WeatherForecast.Application.Common.Results;

/// <summary>
/// Represents the result of an operation with optional data.
/// </summary>
/// <typeparam name="T">The type of data contained in the result.</typeparam>
public class Result<T>
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The HTTP status code associated with the result.
    /// </summary>
    public StatusCode StatusCode { get; set; }

    /// <summary>
    /// An optional message describing the result.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// A list of error messages if the operation failed.
    /// </summary>
    public List<string> Errors { get; set; } = [];

    /// <summary>
    /// The data returned by the operation if successful.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Creates a successful result with data.
    /// </summary>
    /// <param name="data">The data to include in the result.</param>
    /// <param name="message">An optional success message.</param>
    /// <param name="statusCode">The HTTP status code (defaults to OK).</param>
    /// <returns>A successful result containing the data.</returns>
    public static Result<T> SuccessResponse(T data, string? message = null, StatusCode statusCode = StatusCode.OK)
    {
        return new Result<T>
        {
            Success = true,
            StatusCode = statusCode,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Creates an error result.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code (defaults to BadRequest).</param>
    /// <param name="errors">An optional list of additional error messages.</param>
    /// <returns>An error result.</returns>
    public static Result<T> ErrorResponse(string message, StatusCode statusCode = StatusCode.BadRequest, List<string>? errors = null)
    {
        return new Result<T>
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors ?? []
        };
    }

    /// <summary>
    /// Creates an error result from FluentValidation validation errors.
    /// </summary>
    /// <param name="validationResult">The FluentValidation validation result.</param>
    /// <param name="validationFailedMessage">The message to display for validation failure (optional).</param>
    /// <returns>An error result containing validation errors.</returns>
    public static Result<T> ValidationError(FluentValidation.Results.ValidationResult validationResult, string validationFailedMessage)
    {
        var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
        return ErrorResponse(validationFailedMessage, StatusCode.BadRequest, errors);
    }
}

