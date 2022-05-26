namespace Flaminco.EnterpriseValidator;

/// <summary>
/// Represents the result of running validation rules.
/// </summary>
/// <typeparam name="T">
/// A datatype that matches the datatype that you want to run a validation rule against.
/// </typeparam>
/// <param name="ValidationMessage">The validation message that should appear to the user when breaking the validation rule. </param>
/// <param name="ValidationRuleName">The validation rule name</param>
public record ValidationRuleResult<T>(string ValidationMessage, string ValidationRuleName);

