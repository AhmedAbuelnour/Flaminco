namespace Flaminco.CommitResult;

public interface ICommitResult
{
    string? ErrorMessage { get; set; }
    string? ErrorCode { get; set; }
    ResultType ResultType { get; set; }
    bool IsSuccess { get; }
}

public interface ICommitResult<T> : ICommitResult
{
    T? Value { get; set; }
}

public interface ICommitResults<T> : ICommitResult
{
    IEnumerable<T>? Value { get; set; }
}

