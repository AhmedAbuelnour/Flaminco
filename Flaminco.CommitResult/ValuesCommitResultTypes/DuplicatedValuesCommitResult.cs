namespace Flaminco.CommitResult.ValueCommitResultTypes;

public class DuplicatedValuesCommitResult<T> : ICommitResults<T>
{
    public DuplicatedValuesCommitResult(string? errorCode, string? errorMessage)
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
