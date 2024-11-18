namespace Flaminco.MinimalMediatR.Options
{
    public interface IValidationExceptionHandlerOptions
    {
        string Type { get; set; }
        string Title { get; set; }
        string UserIdTokenName { get; set; }
    }

    internal class ValidationExceptionHandlerOptions : IValidationExceptionHandlerOptions
    {
        public string Type { get; set; } = "Bad Request"; // Default value
        public string Title { get; set; } = "Validation Failed"; // Default value
        public string UserIdTokenName { get; set; } = "sub"; // Default claim or header to extract user ID
    }
}
