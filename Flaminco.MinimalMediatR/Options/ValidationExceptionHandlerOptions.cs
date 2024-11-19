
using FluentValidation;


namespace Flaminco.MinimalMediatR.Options
{
    internal class ValidationExceptionHandlerOptions : IExceptionHandlerOptions<ValidationException>
    {
        public string Type { get; set; } = "Bad Request"; // Default value
        public string Title { get; set; } = "Validation Failed"; // Default value
        public string UserIdTokenName { get; set; } = "sub"; // Default claim or header to extract user ID
    }
}