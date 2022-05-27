namespace Flaminco.CommitResult.ValueCommitResultsTypes;

public class DuplicatedValueCommitResults<T> : ICommitResults<T>
{
    public DuplicatedValueCommitResults(string? errorCode, string? errorMessage)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public ResultType ResultType { get; set; } = ResultType.Duplicated;
    public bool IsSuccess => string.IsNullOrEmpty(ErrorCode) && string.IsNullOrEmpty(ErrorMessage);
    public IEnumerable<T>? Value { get; set; } = Array.Empty<T>();
}
