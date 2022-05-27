namespace Flaminco.CommitResult.ValueCommitResultsTypes;

public class SuccessValueCommitResults<T> : ICommitResults<T>
{
    public SuccessValueCommitResults(IEnumerable<T>? _value)
    {
        Value = _value;
    }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public ResultType ResultType { get; set; } = ResultType.Ok;
    public bool IsSuccess => string.IsNullOrEmpty(ErrorCode) && string.IsNullOrEmpty(ErrorMessage);
    public IEnumerable<T>? Value { get; set; }
}
