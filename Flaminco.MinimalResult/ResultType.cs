namespace Flaminco.Results;

public enum ResultType
{
    Ok,
    /// <summary>
    /// Used for business errors
    /// </summary>
    UnprocessableEntity,
    /// <summary>
    /// Used for data errors
    /// </summary>
    BadRequest,
    /// <summary>
    /// For duplicate date, or any kind of data that may led to an incorrect state for data
    /// </summary>
    Conflict,
    NotFound,
}
