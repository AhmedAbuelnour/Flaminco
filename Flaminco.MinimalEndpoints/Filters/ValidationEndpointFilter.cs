using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace Flaminco.MinimalEndpoints.Filters
{
    internal sealed class ValidationEndpointFilter<TRequest>(IEnumerable<IValidator<TRequest>> validators) : IEndpointFilter where TRequest : notnull
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            // Try to get the request from the arguments
            TRequest? request = context.Arguments.OfType<TRequest>().FirstOrDefault();

            if (request == null || !validators.Any())
            {
                return await next(context);
            }

            ValidationContext<TRequest> validationContext = new(request);

            ValidationResult[] validationResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(validationContext)));

            List<ValidationFailure> failures = [.. validationResults
                .SelectMany(result => result.Errors)
                .Where(failure => failure != null)];

            if (failures.Count != 0)
            {
                // Option 1: Return validation problem details
                var errors = failures.ToDictionary(
                    failure => failure.PropertyName,
                    failure => new[] { failure.ErrorMessage }
                );

                throw new ValidationException(failures);
            }

            return await next(context);
        }
    }
}
