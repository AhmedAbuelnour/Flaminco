namespace Flaminco.CommitResult.ValueCommitResultTypes;

public class InvalidValuesCommitResult<T> : ICommitResults<T>
{
    public InvalidValuesCommitResult(string? errorCode, string? errorMessage)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public ResultType ResultType { get; set; } = ResultType.Invalid;
    public bool IsSuccess => string.IsNullOrEmpty(ErrorCode) && string.IsNullOrEmpty(ErrorMessage);

    public IEnumerable<T>? Value { get; set; } = Array.Empty<T>();

}
