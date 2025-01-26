using Flaminco.MinimalMediatR.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.MinimalMediatR.Extensions
{
    public static class LoggerExtensions
    {
        public static IServiceCollection AddLoggingPipelineBehavior(this IServiceCollection services)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));

            return services;
        }
    }
}
