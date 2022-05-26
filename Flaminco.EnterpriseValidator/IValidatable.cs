namespace Flaminco.EnterpriseValidator;

public interface IValidatable<T>
{
    List<IValidationRule<T>> ValidationsRules { get; }
    List<ValidationRuleResult<T>> Errors { get; }
    bool Validate();
    bool IsValid { get; }
    string GetValidationRuleResultsAsJson();
    IEnumerable<ValidationRuleResult<T>> GetValidationRuleResults();
}