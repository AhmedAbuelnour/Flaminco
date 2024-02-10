namespace Flaminco.Resultify
{
    /// <summary>
    /// Represents a detailed error within an application, including a code, description, and type of error.
    /// </summary>
    public record Error
    {
        /// <summary>
        /// Gets the unique code identifying the error.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the description of the error, providing more detail about what went wrong.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the type of the error, categorizing it among predefined error types.
        /// </summary>
        public ErrorType Type { get; }

        /// <summary>
        /// Initializes a new instance of the Error record with the specified error code, description, and error type.
        /// </summary>
        /// <param name="code">The unique error code.</param>
        /// <param name="description">The detailed description of the error.</param>
        /// <param name="errorType">The type of the error.</param>
        private Error(string code, string description, ErrorType errorType)
        {
            Code = code;
            Description = description;
            Type = errorType;
        }

        /// <summary>
        /// Provides an implicit conversion from an Error object to a Result object, allowing an Error instance to be used directly where a Result is expected. This facilitates the creation of failure results directly from Error instances, streamlining error handling by eliminating the need for explicit conversion.
        /// </summary>
        /// <param name="error">The Error instance to convert into a Result object.</param>
        /// <returns>A Result object representing a failure, initialized with the provided Error instance.</returns>
        public static implicit operator Result(Error error) => Result.Failure(error);


        /// <summary>
        /// Creates an Error instance representing a not found condition.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="description">The error description.</param>
        /// <returns>An Error instance of type NotFound.</returns>
        public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);

        /// <summary>
        /// Creates an Error instance representing a validation error.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="description">The error description.</param>
        /// <returns>An Error instance of type Validation.</returns>
        public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);

        /// <summary>
        /// Creates an Error instance representing a conflict.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="description">The error description.</param>
        /// <returns>An Error instance of type Conflict.</returns>
        public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);

        /// <summary>
        /// Creates an Error instance representing a general failure.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="description">The error description.</param>
        /// <returns>An Error instance of type Failure.</returns>
        public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);

        /// <summary>
        /// Represents an error instance that signifies the absence of an error.
        /// </summary>
        public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);
    }
}
