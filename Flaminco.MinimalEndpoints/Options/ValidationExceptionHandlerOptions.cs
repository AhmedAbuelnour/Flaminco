using FluentValidation;


namespace Flaminco.MinimalEndpoints.Options
{
    internal sealed class ValidationExceptionHandlerOptions : IExceptionHandlerOptions<ValidationException>
    {
        /// <inheritdoc />
        public string Type { get; set; } = "Bad Request"; // Default value
        /// <inheritdoc />
        public string Title { get; set; } = "Validation Failed"; // Default value
        /// <inheritdoc />
        public string UserIdentifierTokenName { get; set; } = "sub"; // Default claim or header to extract user ID
    }
}