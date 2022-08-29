namespace Flaminco.CommitResult.ValueCommitResultTypes;

public class DuplicatedValueCommitResult<TValue> : CommitResult<TValue>
{
    public DuplicatedValueCommitResult(TValue? value, string? errorCode, string? errorMessage) : base(value, errorCode, errorMessage)
    {
        ResultType = ResultType.Duplicated;
    }
}
