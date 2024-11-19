using Flaminco.MinimalMediatR.Behaviors;
using Flaminco.MinimalMediatR.Exceptions;
using Flaminco.MinimalMediatR.Options;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.MinimalMediatR.Extensions
{
    public static class ValidationExtensions
    {
        public static IServiceCollection AddValidationProblemHandler(this IServiceCollection services, Action<IExceptionHandlerOptions<ValidationException>>? options = default)
        {
            IExceptionHandlerOptions<ValidationException> config = new ValidationExceptionHandlerOptions();

            options?.Invoke(config);

            services.AddSingleton(config);

            services.AddExceptionHandler<ValidationsExceptionHandler>();

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));

            return services;
        }
    }
}
