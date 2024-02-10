using Flaminco.Validation.Models;

namespace Flaminco.Validation.Abstractions;

public interface IValidation
{
    Task<Result> ValidateAsync<TInput>(TInput input, CancellationToken cancellationToken = default) where TInput : notnull;

    Result Validate<TInput>(TInput input) where TInput : notnull;
}