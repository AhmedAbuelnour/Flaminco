using Flaminco.ImmutableStates.Abstractions;
using Flaminco.ImmutableStates.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Flaminco.ImmutableStates.Extensions
{
    public static class ImmutableStateExtensions
    {
        /// <summary>
        /// Adds the immutable state lookup service to the dependency injection container.
        /// </summary>
        /// <typeparam name="TContext">The type of the DbContext used for database operations.</typeparam>
        /// <param name="services">The service collection to add the service to.</param>
        /// <returns>The service collection with the added service.</returns>
        public static IServiceCollection AddImmutableState<TContext>(this IServiceCollection services) where TContext : DbContext
            => services.AddSingleton<IImmutableStateLookup, DefaultImmutableStateLookup<TContext>>();
    }
}
