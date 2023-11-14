using Flaminco.Validation.Models;

namespace Flaminco.Validation.Abstractions;

public interface IValidationAsyncHandler<TInput>
{
    Task<Result> Handler(TInput input, CancellationToken cancellationToken = default);
}
