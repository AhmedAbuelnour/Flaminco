namespace Flaminco.CommitResult.ValueCommitResultTypes;

public class SuccessValueCommitResult<TValue> : CommitResult<TValue> {
    public SuccessValueCommitResult(TValue? value, string? errorCode = default, string? errorMessage = default) : base(value, errorCode, errorMessage) {
        ResultType = ResultType.Ok;
    }
}