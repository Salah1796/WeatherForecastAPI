namespace WeatherForecast.Application.Common.Enums;

public enum StatusCode
{
    // Success
    OK = 200,
    Created = 201,
    NoContent = 204,

    // Client Error
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    Conflict = 409,
    UnprocessableEntity = 422,

    // Server Error
    InternalServerError = 500,
    BadGateway = 502,
    ServiceUnavailable = 503
}

