using Flaminco.MinimalMediatR.Behaviors;
using Flaminco.MinimalMediatR.Exceptions;
using Flaminco.MinimalMediatR.Options;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.MinimalMediatR.Extensions
{
    public static class ValidationExtensions
    {
        public static IServiceCollection AddValidationProblemHandler(this IServiceCollection services, Action<IValidationExceptionHandlerOptions>? options = default)
        {
            IValidationExceptionHandlerOptions config = new ValidationExceptionHandlerOptions();

            options?.Invoke(config);

            services.AddSingleton(config);

            services.AddExceptionHandler<ValidationsExceptionHandler>();

            services.AddProblemDetails();

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));

            return services;
        }
    }
}
