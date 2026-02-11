using Flaminco.MinimalEndpoints.Models;

namespace Flaminco.MinimalEndpoints.Abstractions
{
    public interface IMinimalValidator<TRequest>
    {
        IEnumerable<ValidationFailure> Validate(TRequest request);
    }

    public interface IAsyncMinimalValidator<in TRequest>
    {
        IAsyncEnumerable<ValidationFailure> ValidateAsync(TRequest request, CancellationToken cancellationToken = default);
    }
}
