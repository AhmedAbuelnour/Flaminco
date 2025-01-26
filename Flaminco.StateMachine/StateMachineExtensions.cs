using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Flaminco.StateMachine
{

    /// <summary>
    /// Provides extension methods for adding state machine services to the IServiceCollection.
    /// </summary>
    public static class StateMachineExtensions
    {
        /// <summary>
        /// Adds state machine services to the IServiceCollection.
        /// </summary>
        /// <typeparam name="TStateScanner">The type used to scan for state types.</typeparam>
        /// <param name="services">The IServiceCollection to add the services to.</param>
        public static void AddStateMachine<TStateScanner>(this IServiceCollection services)
        {
            foreach (TypeInfo? stateType in typeof(TStateScanner).Assembly
                                                  .DefinedTypes
                                                  .Where(type => !type.IsAbstract &&
                                                                  type.BaseType != null &&
                                                                  type.BaseType.IsGenericType &&
                                                                  type.BaseType.GetGenericTypeDefinition() == typeof(State<>)))
            {
                if (stateType.GetCustomAttribute<StateKeyAttribute>() is StateKeyAttribute stateKeyAttribute)
                {
                    services.AddKeyedTransient(stateType.BaseType!, stateKeyAttribute.Key, stateType);
                }
            }

            services.AddTransient(typeof(StateContext<>));
        }
    }
}
