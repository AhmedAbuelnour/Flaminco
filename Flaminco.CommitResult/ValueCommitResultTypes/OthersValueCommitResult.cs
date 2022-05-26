namespace Flaminco.CommitResult.ValueCommitResultTypes;

public class OthersValueCommitResult<T> : ICommitResult<T>
{
    public OthersValueCommitResult(string? errorCode, string? errorMessage)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public ResultType ResultType { get; set; } = ResultType.Others;
    public bool IsSuccess => string.IsNullOrEmpty(ErrorCode) && string.IsNullOrEmpty(ErrorMessage);
    public T? Value { get; set; } = default;

}