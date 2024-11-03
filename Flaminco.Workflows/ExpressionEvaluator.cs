using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Flaminco.Workflows;

internal static class ExpressionEvaluator
{
    private static readonly ParsingConfig _parsingConfig = new()
    {
        ResolveTypesBySimpleName = true
    };

    private static readonly Dictionary<string, Delegate> _lambdaCache = [];

    internal static object? Evaluate(string expression, Dictionary<string, object> parameters)
    {
        if (!_lambdaCache.TryGetValue(expression, out var compiledLambda))
        {
            // Compile the lambda expression if not in cache
            var parameterExpressions =
                parameters.Select(kvp => Expression.Parameter(kvp.Value.GetType(), kvp.Key)).ToArray();

            var lambda = DynamicExpressionParser.ParseLambda(
                _parsingConfig,
                parameterExpressions,
                null,
                expression,
                [.. parameters.Values]);

            compiledLambda = lambda.Compile();
            _lambdaCache[expression] = compiledLambda;
        }

        // Execute the lambda expression whether it was just compiled or retrieved from cache
        var parameterValues = parameters.Select(p => p.Value).ToArray();

        return compiledLambda.DynamicInvoke(parameterValues);
    }
}