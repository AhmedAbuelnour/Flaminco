using Flaminco.MinimalEndpoints.ExceptionHandlers;
using Flaminco.MinimalEndpoints.Exceptions;
using Flaminco.MinimalEndpoints.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.MinimalEndpoints.Extensions
{
    public static class ExceptionHandlerExtensions
    {
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
