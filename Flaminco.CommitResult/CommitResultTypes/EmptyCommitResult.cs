namespace Flaminco.CommitResult.CommitResultTypes;

public class EmptyCommitResult : ICommitResult
{
    public EmptyCommitResult(string? errorCode, string? errorMessage)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public ResultType ResultType { get; set; } = ResultType.Empty;
    public bool IsSuccess => string.IsNullOrEmpty(ErrorCode) && string.IsNullOrEmpty(ErrorMessage);
}
