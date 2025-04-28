namespace Flaminco.MinimalEndpoints.Options
{
    using System;
    /// <summary>
    /// Defines options for handling exceptions of a specific type.
    /// </summary>
    /// <typeparam name="TException">The type of exception to handle.</typeparam>
    public interface IExceptionHandlerOptions<TException> where TException : Exception
    {
        /// <summary>
        /// Gets or sets the type of the exception handler.
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// Gets or sets the title associated with the exception handler.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets the name of the token used to identify the user.
        /// </summary>
        string UserIdentifierTokenName { get; set; }
    }
}
