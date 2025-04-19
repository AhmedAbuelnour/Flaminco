using Flaminco.MinimalEndpoints.ExceptionHandlers;
using Flaminco.MinimalEndpoints.Exceptions;
using Flaminco.MinimalEndpoints.Options;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.MinimalEndpoints.Extensions
{
    public static class ExceptionHandlerExtensions
    {
        public static IServiceCollection AddValidationExceptionHandler<TValidationRuleScanner>(this IServiceCollection services, Action<IExceptionHandlerOptions<ValidationException>>? options = default)
        {
            IExceptionHandlerOptions<ValidationException> config = new ValidationExceptionHandlerOptions();

            options?.Invoke(config);

            services.AddSingleton(config);

            services.AddExceptionHandler<ValidationsExceptionHandler>();

            services.AddValidatorsFromAssemblyContaining<TValidationRuleScanner>();

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
