using ErrorOr;

namespace Flaminco.MinimalEndpoints.Models
{
    public class EndpointResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }

        // Add Status Method that represents the success or failure of the endpoint result
        public static EndpointResult Success() => new()
        {
            IsSuccess = true
        };

        public static EndpointResult Failure(string errorCode, string errorMessage) => new()
        {
            IsSuccess = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage
        };

        public static EndpointResult Failure(Error error) => new()
        {
            IsSuccess = false,
            ErrorCode = error.Code,
            ErrorMessage = error.Description
        };
    }

    public class EndpointResult<T> : EndpointResult
    {
        public T? Value { get; set; }
        public static EndpointResult<T> Success(T value) => new()
        {
            IsSuccess = true,
            Value = value
        };
        public static EndpointResult<T?> Failure(string errorCode, string errorMessage, T? value = default) => new()
        {
            IsSuccess = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            Value = value
        };

        public static EndpointResult<T?> Failure(Error error, T? value = default) => new()
        {
            IsSuccess = false,
            ErrorCode = error.Code,
            ErrorMessage = error.Description,
            Value = value
        };
    }
}
