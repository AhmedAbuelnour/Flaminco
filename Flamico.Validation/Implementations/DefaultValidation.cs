using Flaminco.ManualMapper.Abstractions;
using Flaminco.Validation.Abstractions;
using Flaminco.Validation.Exceptions;
using Flaminco.Validation.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.ManualMapper.Implementations;

public sealed class DefaultValidation(IServiceProvider serviceProvider) : IValidation
{
    public Result Validate<TInput>(TInput input) where TInput : notnull
    {
        ArgumentNullException.ThrowIfNull(input);

        IValidationHandler<TInput>? handler = serviceProvider.GetService<IValidationHandler<TInput>>();

        return handler switch
        {
            null => throw new ValidationHandlerNotRegisteredException<IValidationHandler<TInput>>(),
            _ => handler.Handler(input)
        };
    }

    public Task<Result> ValidateAsync<TInput>(TInput input, CancellationToken cancellationToken = default) where TInput : notnull
    {
        ArgumentNullException.ThrowIfNull(input);

        IValidationAsyncHandler<TInput>? handler = serviceProvider.GetService<IValidationAsyncHandler<TInput>>();

        return handler switch
        {
            null => throw new ValidationHandlerNotRegisteredException<IValidationAsyncHandler<TInput>>(),
            _ => handler.Handler(input, cancellationToken)
        };
    }
}
