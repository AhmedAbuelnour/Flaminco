using Flaminco.StateMachine.Abstractions;
using Flaminco.StateMachine.Implementations;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Flaminco.StateMachine.Extensions;

public static class StateMachineExtension
{
    public static IServiceCollection AddStateMachine<TAssemblyScanner>(this IServiceCollection services)
    {

        IEnumerable<TypeInfo> types = from type in typeof(TAssemblyScanner).Assembly.DefinedTypes
                                      where !type.IsAbstract && typeof(IState).IsAssignableFrom(type)
                                      select type.GetTypeInfo();

        foreach (TypeInfo? typeInfo in types)
        {
            foreach (var implementedInterface in typeInfo.ImplementedInterfaces)
            {
                services.AddScoped(implementedInterface, typeInfo);
            }
        }

        services.AddScoped<IStateContext, DefaultStateContext>();


        return services;
    }
}
