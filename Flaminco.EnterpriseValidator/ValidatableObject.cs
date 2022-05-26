using System.Text.Json;

namespace Flaminco.EnterpriseValidator;

/// <summary>
/// A base implementation for IValidatable interface.
/// </summary>
/// <typeparam name="T">
/// A datatype that matches the datatype that you want to run a validation rule against
/// </typeparam>
public class ValidatableObject<T> : IValidatable<T>
{
    public ValidatableObject()
    {
        ValidationsRules = new List<IValidationRule<T>>();
        Errors = new List<ValidationRuleResult<T>>();
    }
    public List<IValidationRule<T>> ValidationsRules { get; }
    public List<ValidationRuleResult<T>> Errors { get; }
    public bool IsValid { get; private set; }
    public T Value { get; set; }

    /// <summary>
    /// Add Rules to run against your input
    /// </summary>
    /// <param name="rules">params of your rules</param>
    public void AddRules(params IValidationRule<T>[] rules)
    {
        if (rules == null)
            throw new ArgumentNullException(nameof(rules), "You have't set any validation rules");

        if (rules.Any(a => string.IsNullOrWhiteSpace(a.ValidationMessage)))
            throw new ArgumentNullException(nameof(rules), $"One or more of the validation rules has no validation message");

        ValidationsRules.AddRange(rules);
    }
    public bool Validate()
    {
        if (Value is null)
            throw new NullReferenceException($"You have't set a value for the property {nameof(Value)}");

        ValidationsRules.ForEach(async a =>
        {
            if (!await a.Check(Value))
                Errors.Add(new ValidationRuleResult<T>(a.ValidationMessage, string.Concat((a.GetType().Name).Where(a => char.IsLetter(a)))));
        });
        return IsValid = Errors.Count == 0;
    }
    public string GetValidationRuleResultsAsJson()
        => JsonSerializer.Serialize(Errors);

    public IEnumerable<ValidationRuleResult<T>> GetValidationRuleResults() => Errors;
}

