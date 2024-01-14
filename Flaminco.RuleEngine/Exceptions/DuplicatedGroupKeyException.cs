namespace Flaminco.RuleEngine.Exceptions
{
    internal class DuplicatedGroupKeyException(string groupKey) : Exception($"Duplicated group keys are not allowed, GroupKey: {groupKey} is already used within the same workflow")
    {
    }
}
