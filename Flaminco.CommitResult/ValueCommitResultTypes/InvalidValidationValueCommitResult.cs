namespace Flaminco.CommitResult.ValueCommitResultTypes;

public class InvalidValidationValueCommitResult<T> : ICommitResult<T>
{
    public InvalidValidationValueCommitResult(string? errorCode, string? errorMessage)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public ResultType ResultType { get; set; } = ResultType.InvalidValidation;
    public bool IsSuccess => string.IsNullOrEmpty(ErrorCode) && string.IsNullOrEmpty(ErrorMessage);
    public T? Value { get; set; } = default;

}
