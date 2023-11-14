using Flaminco.ManualMapper.Abstractions;
using Flaminco.Validation.Abstractions;
using Flaminco.Validation.Models;

namespace Flaminco.ManualMapper.Implementations;

public sealed class DefaultValidation : IValidation
{
    private readonly IServiceProvider _serviceProvider;
    public DefaultValidation(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;


    public Result Validate<TInput>(TInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        Type profileType = typeof(IValidationHandler<>).MakeGenericType(typeof(TInput));

        object? handler = _serviceProvider.GetService(profileType);

        ArgumentNullException.ThrowIfNull(handler);

        return handler.GetType().GetMethod("Handler")?.Invoke(handler, new object[] { input }) switch
        {
            Result destination => destination,
            _ => throw new InvalidOperationException($"{nameof(handler)} is not registered")
        };
    }

    public Task<Result> ValidateAsync<TInput>(TInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        Type profileType = typeof(IValidationAsyncHandler<>).MakeGenericType(typeof(TInput));

        object? handler = _serviceProvider.GetService(profileType);

        ArgumentNullException.ThrowIfNull(handler);

        return handler.GetType().GetMethod("Handler")?.Invoke(handler, new object[] { input, cancellationToken }) switch
        {
            Task<Result> destination => destination,
            _ => throw new InvalidOperationException($"{nameof(handler)} is not registered")
        };
    }
}
