namespace Flaminco.CommitResult.ValueCommitResultTypes;

public class DuplicatedValueCommitResult<T> : ICommitResult<T>
{
    public DuplicatedValueCommitResult(string? errorCode, string? errorMessage)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public ResultType ResultType { get; set; } = ResultType.Duplicated;
    public bool IsSuccess => string.IsNullOrEmpty(ErrorCode) && string.IsNullOrEmpty(ErrorMessage);
    public T? Value { get; set; } = default;
}
