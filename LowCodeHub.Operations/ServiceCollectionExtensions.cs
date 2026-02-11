using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LowCodeHub.Operations;

/// <summary>
/// Registers all operation handlers (types implementing IOperation&lt;,&gt;) from an assembly.
/// Call once per use-case assembly so endpoints can inject operations.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Finds all types that implement IOperation&lt;TRequest, TResponse&gt; and registers them as scoped.
    /// Use with the source generator: generated interfaces (e.g. IUploadAttachmentOperation) are implemented
    /// by your [Operation] class, so both the generic and the short-named interface get the same implementation.
    /// </summary>
    public static IServiceCollection AddOperations(this IServiceCollection services, Assembly assembly)
    {
        var operationType = typeof(IOperation<,>);

        foreach (var type in assembly.GetTypes())
        {
            if (type is not { IsClass: true, IsAbstract: false })
                continue;

            foreach (var iface in type.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == operationType)
                {
                    services.AddScoped(iface, type);
                    break;
                }
            }
        }

        return services;
    }
}
