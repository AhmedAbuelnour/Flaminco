using Flaminco.Results.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Flaminco.Results.Implementations;

public sealed class ValidatorEndpointFilter<TValue> : IEndpointFilter
{
    private readonly IValidator<TValue> _validator;

    public ValidatorEndpointFilter(IValidator<TValue> validator) => _validator = validator;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        TValue? input = context.GetArgument<TValue>(0);

        if (input is not null)
        {
            var validationResult = await _validator.ValidateAsync(input, CancellationToken.None);

            if (!validationResult.IsValid)
            {
                return ResultType.BadRequest.GetMinimalResult(errorDetails: validationResult.ToDictionary());
            }
        }

        return await next.Invoke(context);
    }
}
