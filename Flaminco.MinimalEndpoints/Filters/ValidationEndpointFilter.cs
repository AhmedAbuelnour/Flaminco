using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace Flaminco.MinimalEndpoints.Filters
{
    /// <summary>
    /// Represents an endpoint filter that validates a request using FluentValidation.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request to validate.</typeparam>
    /// <param name="validators">A collection of validators for the request type.</param>
    internal sealed class ValidationEndpointFilter<TRequest>(IEnumerable<IValidator<TRequest>> validators) : IEndpointFilter where TRequest : notnull
    {
        /// <summary>
        /// Invokes the endpoint filter to validate the request and proceed to the next delegate if valid.
        /// </summary>
        /// <param name="context">The context of the endpoint invocation.</param>
        /// <param name="next">The next delegate in the filter pipeline.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response object or null.</returns>
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
                throw new ValidationException(failures);
            }

            return await next(context);
        }
    }
}
