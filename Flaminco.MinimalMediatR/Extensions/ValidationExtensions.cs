using Flaminco.MinimalMediatR.Behaviors;
using Flaminco.MinimalMediatR.ExceptionHandlers;
using Flaminco.MinimalMediatR.Options;
using Flaminco.Validation.Options;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.MinimalMediatR.Extensions
{
    public static class ValidationExtensions
    {
        public static IServiceCollection AddValidationExceptionHandler(this IServiceCollection services, Action<IExceptionHandlerOptions<ValidationException>>? options = default)
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
