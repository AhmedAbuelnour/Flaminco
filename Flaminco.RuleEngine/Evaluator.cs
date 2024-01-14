namespace Flaminco.RuleEngine
{
    using Flaminco.RuleEngine.Exceptions;
    using Flaminco.RuleEngine.Models;

    public class Evaluator
    {
        private static KeyValuePair<string, object?> EvaluateRule(Rule rule)
        {
            try
            {
                bool? evaluateResult = ExpressionEvaluator.Evaluate(rule.Expression, rule.Inputs);

                if (evaluateResult.HasValue)
                {
                    if (rule.OutputType == OutputType.Expression && evaluateResult == true)
                    {
                        if (string.IsNullOrWhiteSpace(rule.ExpressionOutput))
                        {
                            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(rule.ExpressionOutput));
                        }

                        return new KeyValuePair<string, object?>(rule.Key, ExpressionEvaluator.Evaluate(rule.ExpressionOutput!, rule.Inputs));
                    }

                    return new KeyValuePair<string, object?>(rule.Key, evaluateResult == true ? rule.SuccessOutput : rule.FailureOutput);
                }
                else
                {
                    throw new BooleanEvaluationException(rule.Expression);
                }
            }
            catch
            {
                // Handle or log the exception as needed
                throw;
            }
        }
        private static Dictionary<string, object?> EvaluateRuleGroup(RuleGroup group)
        {
            return group.Rules.OrderBy(a => a.Order).Select(EvaluateRule).ToDictionary();
        }


        public static Dictionary<string, object?> EvaluateRules(Workflow workflow)
        {
            if (workflow is null) ArgumentException.ThrowIfNullOrEmpty(nameof(workflow));

            if (workflow!.WorkflowType == WorkflowType.Rules && workflow.Rules is not null && workflow.Rules.Length > 0)
            {
                Dictionary<string, object?> ruleResults = [];

                foreach (var rule in workflow.Rules.OrderBy(a => a.Order))
                {
                    if (EvaluateRule(rule) is KeyValuePair<string, object?> evaluationResult)
                    {
                        ruleResults.TryAdd(evaluationResult.Key, evaluationResult.Value);
                    }
                }

                return ruleResults;
            }
            else
            {
                throw new EmptyRulesException(workflow.Name);
            }

        }

        public static Dictionary<string, Dictionary<string, object?>> EvaluateGroups(Workflow workflow)
        {
            if (workflow is null) ArgumentException.ThrowIfNullOrEmpty(nameof(workflow));

            if (workflow!.WorkflowType == WorkflowType.Groups && workflow.RuleGroups is not null && workflow.RuleGroups.Length > 0)
            {
                Dictionary<string, Dictionary<string, object?>> groupResults = [];

                foreach (RuleGroup ruleGroup in workflow.RuleGroups.OrderBy(a => a.Order))
                {
                    if (EvaluateRuleGroup(ruleGroup) is Dictionary<string, object?> groupResult)
                    {
                        if (!groupResults.TryAdd(ruleGroup.GroupKey, groupResult))
                        {
                            throw new DuplicatedGroupKeyException(ruleGroup.GroupKey);
                        }
                    }
                }
                return groupResults;
            }
            else
            {
                throw new EmptyRulesException(workflow.Name);
            }
        }
    }

}
