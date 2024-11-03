using Flaminco.Workflows.Exceptions;
using Flaminco.Workflows.Models;

namespace Flaminco.Workflows;

public class Evaluator
{
    public static Dictionary<string, object?> EvaluateRules(Workflow workflow)
    {
        if (workflow is null) ArgumentException.ThrowIfNullOrEmpty(nameof(workflow));

        if (workflow!.WorkflowType == WorkflowType.Rules && workflow.Rules is not null && workflow.Rules.Length > 0)
        {
            Dictionary<string, object?> ruleResults = [];

            foreach (var rule in workflow.Rules.OrderBy(a => a.Order))
                if (Evaluate(rule) is KeyValuePair<string, object?> evaluationResult)
                    ruleResults.TryAdd(evaluationResult.Key, evaluationResult.Value);

            return ruleResults;
        }

        throw new EmptyRulesException(workflow.Name);
    }

    public static Dictionary<string, Dictionary<string, object?>> EvaluateGroups(Workflow workflow)
    {
        if (workflow is null) ArgumentException.ThrowIfNullOrEmpty(nameof(workflow));

        if (workflow!.WorkflowType == WorkflowType.Groups && workflow.RuleGroups is not null &&
            workflow.RuleGroups.Length > 0)
        {
            Dictionary<string, Dictionary<string, object?>> groupResults = [];

            foreach (var ruleGroup in workflow.RuleGroups.OrderBy(a => a.Order))
                if (EvaluateRuleGroup(ruleGroup) is Dictionary<string, object?> groupResult)
                    if (!groupResults.TryAdd(ruleGroup.GroupKey, groupResult))
                        throw new DuplicatedGroupKeyException(ruleGroup.GroupKey);
            return groupResults;
        }

        throw new EmptyRulesException(workflow.Name);
    }

    private static KeyValuePair<string, object?> Evaluate(Rule rule)
    {
        if (ExpressionEvaluator.Evaluate(rule.Expression, rule.Inputs) is bool evaluateResult)
        {
            if (rule.OutputType == OutputType.Expression && evaluateResult)
            {
                if (string.IsNullOrWhiteSpace(rule.ExpressionOutput))
                    ArgumentException.ThrowIfNullOrWhiteSpace(nameof(rule.ExpressionOutput));
                return new KeyValuePair<string, object?>(rule.Key,
                    ExpressionEvaluator.Evaluate(rule.ExpressionOutput!, rule.Inputs));
            }

            return new KeyValuePair<string, object?>(rule.Key,
                evaluateResult ? rule.SuccessOutput : rule.FailureOutput);
        }

        throw new BooleanEvaluationException(rule.Expression);
    }

    private static Dictionary<string, object?> EvaluateRuleGroup(RuleGroup group)
    {
        return group.Rules.OrderBy(a => a.Order).Select(Evaluate).ToDictionary();
    }
}