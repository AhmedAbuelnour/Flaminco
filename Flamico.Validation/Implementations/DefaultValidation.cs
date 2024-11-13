using ErrorOr;
using Flaminco.Validation.Abstractions;
using Flaminco.Validation.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.ManualMapper.Implementations;

/// <summary>
///     Default implementation of the <see cref="IValidation" /> interface.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="DefaultValidation" /> class.
/// </remarks>
/// <param name="serviceProvider">The service provider for resolving validation handlers.</param>
internal sealed class DefaultValidation(IServiceProvider serviceProvider) : IValidation
{
    /// <inheritdoc />
    public ValueTask<ErrorOr<Success>> Validate<TInput>(TInput input, CancellationToken cancellationToken = default)
        where TInput : IValidatableObject
    {
        ArgumentNullException.ThrowIfNull(input);

        return serviceProvider.GetService<IValidationHandler<TInput>>()?.Handler(input, cancellationToken) ??
               throw new ValidationHandlerNotRegisteredException<IValidationHandler<TInput>>();
    }
}