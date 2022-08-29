namespace Flaminco.CommitResult.ValueCommitResultsTypes
{
    public class CommitResults<TValue> : ICommitResults<TValue>
    {
        public CommitResults(IEnumerable<TValue>? value, string? errorCode, string? errorMessage)
        {
            Value = value ?? Array.Empty<TValue>();
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        public ResultType ResultType { get; set; }
        public bool IsSuccess => string.IsNullOrEmpty(ErrorCode) && string.IsNullOrEmpty(ErrorMessage);
        public IEnumerable<TValue>? Value { get; set; }
    }
}
