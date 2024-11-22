using Flaminco.MinimalMediatR.Abstractions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Flaminco.MinimalMediatR.Behaviors
{
    internal class ValidationPipelineBehavior<TRequest, IResult>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, IResult> where TRequest : IEndPoint
    {
        public async Task<IResult> Handle(TRequest request, RequestHandlerDelegate<IResult> next, CancellationToken cancellationToken)
        {
            if (!validators.Any()) return await next();

            ValidationContext<TRequest> context = new(request);

            ValidationResult[] validationResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            List<ValidationFailure> failures = validationResults.SelectMany(result => result.Errors).Where(failure => failure != null).ToList();

            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }

            return await next();
        }
    }
}