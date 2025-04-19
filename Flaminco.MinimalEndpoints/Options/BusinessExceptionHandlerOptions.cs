using Flaminco.MinimalEndpoints.Exceptions;

namespace Flaminco.MinimalEndpoints.Options
{
    /// <summary>
    /// Options for handling business exceptions.
    /// </summary>
    internal sealed class BusinessExceptionHandlerOptions : IExceptionHandlerOptions<BusinessException>
    {
        /// <inheritdoc />
        public string Type { get; set; } // Default value

        /// <inheritdoc />
        public string Title { get; set; } = "A business error occurred"; // Default value

        /// <inheritdoc />
        public string UserIdentifierTokenName { get; set; } = "sub"; // Default claim or header to extract user ID
    }
}
