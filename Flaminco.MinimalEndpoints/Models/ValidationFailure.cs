using Flaminco.MinimalEndpoints.Helpers;

namespace Flaminco.MinimalEndpoints.Models;

public sealed class ValidationFailure
{
    public string PropertyName { get; set; } = default!;

    /// <summary>
    /// Gets or sets the error message. 
    /// If ErrorCode is provided, the ErrorMessage will be automatically localized when accessed.
    /// If ErrorMessageArgs is provided, the localized message will be formatted with those arguments.
    /// </summary>
    public string? ErrorMessage
    {
        get
        {
            // If ErrorCode is provided, try to localize the message
            if (!string.IsNullOrEmpty(ErrorCode))
            {
                var localized = LocalizationHelper.Localize(ErrorCode, field, Args);
                if (localized is not null)
                    return localized;
            }

            return field;
        }
        set => field = value;
    }

    /// <summary>
    /// Gets or sets the error code used as the localization key.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the format arguments for the localized error message.
    /// These arguments will be used with string.Format() to format the localized message.
    /// Example: If ErrorCode is "InvalidEmail" and the JSON contains "InvalidEmail": "Email '{0}' is invalid",
    /// then ErrorMessageArgs = ["test@example.com"] will produce "Email 'test@example.com' is invalid"
    /// </summary>
    public object[]? Args { get; set; }

    /// <summary>
    /// Gets or sets the attempted value that caused the validation failure.
    /// </summary>
    public object? AttemptedValue { get; set; }
}
