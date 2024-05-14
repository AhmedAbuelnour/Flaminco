using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.Cache.Extensions
{
    public static class CacheExtensions
    {
        public static IServiceCollection AddCache(this IServiceCollection services, IConfiguration Configuration)
        {



            return services;
        }
    }
}
