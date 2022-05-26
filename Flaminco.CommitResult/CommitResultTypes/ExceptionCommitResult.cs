namespace Flaminco.CommitResult.CommitResultTypes;

public class ExceptionCommitResult : ICommitResult
{
    public ExceptionCommitResult(string? errorCode, string? errorMessage)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public ResultType ResultType { get; set; } = ResultType.Exception;
    public bool IsSuccess => string.IsNullOrEmpty(ErrorCode) && string.IsNullOrEmpty(ErrorMessage);
}
