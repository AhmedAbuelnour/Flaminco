using Flaminco.Results.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Flaminco.Results.Implementations;

public sealed class ValidatorEndpointFilter<TValue> : IEndpointFilter where TValue : class
{
    private readonly IValidator<TValue> _validator;

    public ValidatorEndpointFilter(IValidator<TValue> validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (context.Arguments.Where(a => a?.GetType() == typeof(TValue))?.FirstOrDefault() is TValue input)
        {
            var validationResult = await _validator.ValidateAsync(input, CancellationToken.None);

            if (!validationResult.IsValid)
                return ResultType.BadRequest.GetMinimalResult(validationResult.ToDictionary());
        }

        return await next.Invoke(context);
    }
}