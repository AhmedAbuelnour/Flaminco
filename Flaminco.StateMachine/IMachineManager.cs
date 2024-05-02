using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Flaminco.StateMachine
{

    public static class StateMachineExtensions
    {
        public static IServiceCollection AddStateMachine<TScanner>(this IServiceCollection services)
        {
            IEnumerable<TypeInfo>? types = from type in typeof(TScanner).Assembly.DefinedTypes
                                           where !type.IsAbstract && typeof(ITransition).IsAssignableFrom(type)
                                           select type.GetTypeInfo();


            foreach (TypeInfo? typeInfo in types ?? [])
            {
                //   services.AddScoped(typeof(ITransition), typeInfo);
            }

            services.AddScoped<IMachine, Machine>();


            //services.Configure<Machine>(options =>
            //{
            //    options.NewStartState("Start");

            //    options.NewStopState("Stop");

            //    options.NewState("Normal");
            //});
            return services;
        }
    }


}
