using ErrorOr;

namespace Flaminco.MinimalEndpoints.Exceptions
{
    /// <summary>
    /// Represents a business exception with a status code, error code, and additional details.
    /// </summary>
    public class BusinessException : Exception
    {
        /// <summary>
        /// Gets the status code associated with this exception.
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// Gets the unique error code associated with this exception.
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// Gets additional details about the exception.
        /// </summary>
        public string Details { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessException"/> class with a status code, error code, and details.
        /// </summary>
        /// <param name="statusCode">The status code associated with the exception.</param>
        /// <param name="errorCode">The unique error code for the exception.</param>
        /// <param name="details">Additional details about the exception.</param>
        public BusinessException(int statusCode, string errorCode, string details) : base(details)
        {
            ErrorCode = errorCode;
            Details = details;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessException"/> class with a status code and an <see cref="Error"/> object.
        /// </summary>
        /// <param name="statusCode">The status code associated with the exception.</param>
        /// <param name="error">The error object containing the error code and description.</param>
        public BusinessException(int statusCode, Error error) : base(error.Description)
        {
            ErrorCode = error.Code;
            Details = error.Description;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessException"/> class with a status code and an <see cref="Error"/> object.
        /// </summary>
        /// <param name="error">The error object containing the error code and description.</param>
        public BusinessException(Error error) : base(error.Description)
        {
            ErrorCode = error.Code;
            Details = error.Description;
            StatusCode = 601;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessException"/> class with a specified error message, inner exception, status code, error code, and details.
        /// </summary>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        /// <param name="statusCode">The status code associated with the exception.</param>
        /// <param name="errorCode">The unique error code for the exception.</param>
        /// <param name="details">Additional details about the exception.</param>
        public BusinessException(Exception innerException, int statusCode, string errorCode, string details) : base(details, innerException)
        {
            ErrorCode = errorCode;
            Details = details;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Returns a string that represents the current exception.
        /// </summary>
        public override string ToString()
        {
            return $"Status Code: {StatusCode}\nError Code: {ErrorCode}\nDetails: {Details}";
        }
    }
}
