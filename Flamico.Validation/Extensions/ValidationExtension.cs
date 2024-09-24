using ErrorOr;
using Flaminco.ManualMapper.Implementations;
using Flaminco.Validation.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Flaminco.ManualMapper.Extensions;

/// <summary>
/// Provides extension methods for registering validation services.
/// </summary>
public static class ValidationExtension
{
    /// <summary>
    /// Registers all validation handlers in the specified assembly.
    /// </summary>
    /// <typeparam name="T">The type in the assembly to scan for validation handlers.</typeparam>
    /// <param name="services">The service collection to add the handlers to.</param>
    /// <returns>The service collection with validation handlers registered.</returns>
    public static IServiceCollection AddValidation<T>(this IServiceCollection services)
    {
        List<TypeInfo> types = typeof(T).Assembly.DefinedTypes.Where(type => !type.IsAbstract && type.GetInterfaces().Any(i => i.IsGenericType &&
                                                         (i.GetGenericTypeDefinition() == typeof(IValidationHandler<>)))).ToList();

        foreach (var typeInfo in types)
        {
            foreach (var implementedInterface in typeInfo.ImplementedInterfaces)
            {
                services.AddScoped(implementedInterface, typeInfo);
            }
        }

        services.AddScoped<IValidation, DefaultValidation>();

        return services;
    }

    /// <summary>
    /// Converts a collection of <see cref="ValidationResult"/> to an <see cref="ErrorOr{T}"/> result.
    /// </summary>
    /// <param name="validationResults">The validation results to convert.</param>
    /// <returns>An <see cref="ErrorOr{T}"/> result containing validation errors or success.</returns>
    internal static ErrorOr<Success> ConvertToErrorOr(this ICollection<ValidationResult> validationResults)
    {
        if (validationResults == null || validationResults.Count == 0 || validationResults.All(vr => vr == ValidationResult.Success))
        {
            return Result.Success;
        }

        return validationResults.Where(vr => vr != ValidationResult.Success)
                                .SelectMany(vr => vr.MemberNames.Select(memberName =>
                                    Error.Validation(memberName, vr.ErrorMessage ?? "Validation failed.")))
                                .ToList();
    }
}
