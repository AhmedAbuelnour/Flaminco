using ErrorOr;

namespace LowCodeHub.Operations;

/// <summary>
/// Base interface for a single-responsibility operation (replaces service methods).
/// One operation = one use case. Endpoints inject and call ExecuteAsync.
/// </summary>
public interface IOperation<in TRequest, TResponse>
{
    /// <summary>
    /// Executes the operation.
    /// </summary>
    Task<ErrorOr<TResponse>> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}
