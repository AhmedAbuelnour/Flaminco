namespace Flaminco.CommitResult.ValueCommitResultTypes;

public class SuccessValueCommitResult<T> : ICommitResult<T>
{
    public SuccessValueCommitResult(T _value)
    {
        Value = _value;
    }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public ResultType ResultType { get; set; } = ResultType.Ok;
    public bool IsSuccess => string.IsNullOrEmpty(ErrorCode) && string.IsNullOrEmpty(ErrorMessage);
    public T? Value { get; set; }
}
