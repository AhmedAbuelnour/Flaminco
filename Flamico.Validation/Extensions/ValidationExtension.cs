using ErrorOr;
using Flaminco.Validation.Abstractions;
using Flaminco.Validation.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Flaminco.Validation.Extensions;

/// <summary>
///     Provides extension methods for registering validation services.
/// </summary>
public static class ValidationExtension
{
    /// <summary>
    ///     Registers all validation handlers in the specified assembly.
    /// </summary>
    /// <typeparam name="T">The type in the assembly to scan for validation handlers.</typeparam>
    /// <param name="services">The service collection to add the handlers to.</param>
    /// <returns>The service collection with validation handlers registered.</returns>
    public static IServiceCollection AddValidation<T>(this IServiceCollection services)
    {
        services.AddExceptionHandler<ProblemExceptionHandler>();

        services.AddExceptionHandler<ValidationsExceptionHandler>();

        services.AddProblemDetails();


        var assembly = Assembly.GetExecutingAssembly();

        // Register all implementations of IValidator<TModel> as transient
        var validators = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>)),
                (t, i) => new { Implementation = t, Interface = i });

        foreach (var validator in validators)
        {
            services.AddTransient(validator.Interface, validator.Implementation);
        }


        // Register all implementations of IValidationRule<TModel, TProperty>
        var validationRules = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidationRule<,>)),
                (t, i) => new { Implementation = t, Interface = i });

        foreach (var rule in validationRules)
        {
            var attribute = rule.Implementation.GetCustomAttribute<ValidationRuleAttribute>();

            if (attribute != null)
            {
                // Register each rule with a unique name from the attribute
                services.AddKeyedTransient(rule.Interface, attribute.Name, rule.Implementation); // Register the rule type itself
            }
        }

        return services;
    }

    /// <summary>
    ///     Converts a collection of <see cref="ValidationResult" /> to an <see cref="ErrorOr{T}" /> result.
    /// </summary>
    /// <param name="validationResults">The validation results to convert.</param>
    /// <returns>An <see cref="ErrorOr{T}" /> result containing validation errors or success.</returns>
    internal static ErrorOr<Success> ConvertToErrorOr(this ICollection<ValidationResult> validationResults)
    {
        if (validationResults == null || validationResults.Count == 0 ||
            validationResults.All(vr => vr == ValidationResult.Success)) return Result.Success;

        ErrorOr<bool> errorOr = Error.Validation("", "");



        return validationResults.Where(vr => vr != ValidationResult.Success)
            .SelectMany(vr => vr.MemberNames.Select(memberName =>
                Error.Validation(memberName, vr.ErrorMessage ?? "Validation failed.")))
            .ToList();
    }

    public static ErrorOr<Success> TryDataAnnotationValidate<TInput>(this TInput model, ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(model, validationContext, results, validateAllProperties: true);

        return results.ConvertToErrorOr();
    }
}