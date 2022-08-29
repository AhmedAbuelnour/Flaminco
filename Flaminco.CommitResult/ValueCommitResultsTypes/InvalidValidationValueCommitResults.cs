namespace Flaminco.CommitResult.ValueCommitResultsTypes;

public class InvalidValidationValueCommitResults<TValue> : CommitResults<TValue>
{
    public InvalidValidationValueCommitResults(IEnumerable<TValue>? value, string? errorCode, string? errorMessage) : base(value, errorCode, errorMessage)
    {
        ResultType = ResultType.InvalidValidation;
    }
}