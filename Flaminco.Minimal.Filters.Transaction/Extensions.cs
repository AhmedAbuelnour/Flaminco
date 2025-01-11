using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.Minimal.Filters.Transaction
{
    public static class Extensions
    {
        public static IServiceCollection AddTransactionEndpointFilter(this IServiceCollection services)
        {
            services.AddScoped<TransactionEndpointFilter>();

            return services;
        }
    }
}
