using ErrorOr;

namespace Flaminco.Validation.Abstractions;

/// <summary>
///     Defines methods for validating input asynchronously.
/// </summary>
public interface IValidation
{
    /// <summary>
    ///     Validates the specified input asynchronously.
    /// </summary>
    /// <typeparam name="TInput">The type of the input to validate.</typeparam>
    /// <param name="input">The input to validate.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation, containing an <see cref="ErrorOr{T}" /> result indicating
    ///     success or failure.
    /// </returns>
    ValueTask<ErrorOr<Success>> Validate<TInput>(TInput input, CancellationToken cancellationToken = default)
        where TInput : notnull;
}