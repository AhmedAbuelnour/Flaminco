namespace Flaminco.Workflows
{
    using System.Collections.Generic;
    using System.Linq.Dynamic.Core;
    using System.Linq.Expressions;

    internal static class ExpressionEvaluator
    {
        private static readonly ParsingConfig _parsingConfig = new()
        {
            ResolveTypesBySimpleName = true
        };

        private static readonly Dictionary<string, Delegate> _lambdaCache = [];

        internal static object? Evaluate(string expression, Dictionary<string, object> parameters)
        {
            if (!_lambdaCache.TryGetValue(expression, out Delegate? compiledLambda))
            {
                // Compile the lambda expression if not in cache
                ParameterExpression[] parameterExpressions = parameters.Select(kvp => Expression.Parameter(kvp.Value.GetType(), kvp.Key)).ToArray();

                LambdaExpression lambda = DynamicExpressionParser.ParseLambda(
                    _parsingConfig,
                    parameterExpressions,
                    null,
                    expression,
                    [.. parameters.Values]);

                compiledLambda = lambda.Compile();
                _lambdaCache[expression] = compiledLambda;
            }

            // Execute the lambda expression whether it was just compiled or retrieved from cache
            object[] parameterValues = parameters.Select(p => p.Value).ToArray();

            return compiledLambda.DynamicInvoke(parameterValues);
        }

    }

}
