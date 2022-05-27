namespace Flaminco.CommitResult.ValueCommitResultsTypes;

public class InvalidValueCommitResults<T> : ICommitResults<T>
{
    public InvalidValueCommitResults(string? errorCode, string? errorMessage)
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
