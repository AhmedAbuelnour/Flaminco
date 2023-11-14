using Flaminco.Validation.Models;

namespace Flaminco.Validation.Abstractions;

public interface IValidation
{
    Task<Result> ValidateAsync<TInput>(TInput input, CancellationToken cancellationToken = default);
    Result Validate<TInput>(TInput input);
}