using ErrorOr;
using Flaminco.MinimalEndpoints.Helpers;

namespace Flaminco.MinimalEndpoints.Models;

public record EndpointResult
{
    public required bool IsSuccess { get; set; }
    public string? ErrorCode { get; set; }
    public int Status { get; set; }
    public string? Description { get; set; }

    public static EndpointResult Success() => new()
    {
        IsSuccess = true
    };

    /// <summary>
    /// Creates a failure result with localized error message.
    /// </summary>
    /// <param name="errorCode">The error code (used as localization key).</param>
    /// <param name="errorMessage">Fallback error message if localization fails.</param>
    /// <param name="args">Format arguments for the localized message (e.g., {0}, {1}).</param>
    public static EndpointResult Failure(string errorCode, string? errorMessage = default, params object[]? args) => new()
    {
        IsSuccess = false,
        ErrorCode = errorCode,
        Description = LocalizationHelper.Localize(errorCode, errorMessage, args),
    };

    /// <summary>
    /// Creates a failure result from an Error object with localized error message.
    /// </summary>
    /// <param name="error">The error containing code and description.</param>
    /// <param name="args">Format arguments for the localized message (e.g., {0}, {1}).</param>
    public static EndpointResult Failure(Error error) => new()
    {
        IsSuccess = false,
        ErrorCode = error.Code,
        Description = LocalizationHelper.Localize(error.Code, error.Description, error.Metadata?.Select(b => b.Value) ?? []),
    };
}

public sealed record EndpointResult<T> : EndpointResult
{
    public T? Value { get; set; }

    public static EndpointResult<T> Success(T value) => new()
    {
        IsSuccess = true,
        Value = value
    };

    /// <summary>
    /// Creates a failure result with localized error message.
    /// </summary>
    /// <param name="errorCode">The error code (used as localization key).</param>
    /// <param name="errorMessage">Fallback error message if localization fails.</param>
    /// <param name="value">The value to include in the result.</param>
    /// <param name="args">Format arguments for the localized message (e.g., {0}, {1}).</param>
    public static EndpointResult<T?> Failure(string errorCode, string errorMessage, T? value = default, params object[]? args) => new()
    {
        IsSuccess = false,
        ErrorCode = errorCode,
        Description = LocalizationHelper.Localize(errorCode, errorMessage, args),
        Value = value,
    };

    /// <summary>
    /// Creates a failure result from an Error object with localized error message.
    /// </summary>
    /// <param name="error">The error containing code and description.</param>
    /// <param name="value">The value to include in the result.</param>
    public static EndpointResult<T?> Failure(Error error, T? value = default) => new()
    {
        IsSuccess = false,
        ErrorCode = error.Code,
        Description = LocalizationHelper.Localize(error.Code, error.Description, error.Metadata?.Select(b => b.Value) ?? []),
        Value = value,
    };
}
