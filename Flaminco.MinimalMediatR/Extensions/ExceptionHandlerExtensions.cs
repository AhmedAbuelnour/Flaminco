using Flaminco.MinimalMediatR.Behaviors;
using Flaminco.MinimalMediatR.ExceptionHandlers;
using Flaminco.MinimalMediatR.Exceptions;
using Flaminco.MinimalMediatR.Options;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.MinimalMediatR.Extensions
{
    public static class ExceptionHandlerExtensions
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

        public static IServiceCollection AddBusinessExceptionHandler(this IServiceCollection services, Action<IExceptionHandlerOptions<BusinessException>>? options = default)
        {
            IExceptionHandlerOptions<BusinessException> config = new BusinessExceptionHandlerOptions();

            options?.Invoke(config);

            services.AddSingleton(config);

            services.AddExceptionHandler<BusinessExceptionHandler>();

            return services;
        }
    }
}
