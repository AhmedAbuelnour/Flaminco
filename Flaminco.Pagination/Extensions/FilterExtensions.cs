using Flaminco.QueryableExtensions.Specifications;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace Flaminco.QueryableExtensions.Extensions
{
    public static class FilterExtensions
    {
        public static IQueryable<TSource> MultiColumnSearch<TSource>(this IQueryable<TSource> query, string filter, params Expression<Func<TSource, string?>>[] properties)
        {
            if (string.IsNullOrEmpty(filter)) return query;

            ParameterExpression parameter = Expression.Parameter(typeof(TSource), "x");

            Expression? combinedExpression = null;

            ConstantExpression filterConstant = Expression.Constant(filter.ToLower());

            foreach (Expression<Func<TSource, string?>> property in properties)
            {
                // Ensure the property expression is a MemberExpression
                if (property.Body is not MemberExpression memberExpr)
                {
                    throw new ArgumentException($"Expression '{property}' must be a member expression.");
                }

                // Rebuild the member access with the new parameter
                MemberExpression propertyAccess = Expression.MakeMemberAccess(parameter, memberExpr.Member);

                // Add null check: propertyAccess != null
                BinaryExpression isNotNull = Expression.NotEqual(propertyAccess, Expression.Constant(null, typeof(string)));

                // Apply ToLower for case-insensitivity
                MethodInfo? toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes);

                MethodCallExpression toLowerExpression = Expression.Call(propertyAccess, toLowerMethod);

                // Build the Contains method call
                MethodInfo? containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)]);

                MethodCallExpression containsExpression = Expression.Call(toLowerExpression, containsMethod, filterConstant);

                // Combine the null check with the contains expression
                BinaryExpression notNullAndContains = Expression.AndAlso(isNotNull, containsExpression);

                // Combine expressions using OR
                combinedExpression = combinedExpression == null ? notNullAndContains : Expression.OrElse(combinedExpression, notNullAndContains);
            }

            if (combinedExpression == null)
            {
                return query;
            }

            // Build the final lambda expression
            Expression<Func<TSource, bool>> lambda = Expression.Lambda<Func<TSource, bool>>(combinedExpression, parameter);

            // Apply the combined expression to the query
            return query.Where(lambda);
        }

        public static Task<Dictionary<TKey, int>> GroupByWithCountAsync<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, CancellationToken cancellationToken) where TKey : notnull
        {
            return source.GroupBy(keySelector).Select(a => new { Key = a.Key, Count = a.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        }
        public static Task<Dictionary<TKey, long>> GroupByWithLongCountAsync<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, CancellationToken cancellationToken) where TKey : notnull
        {
            return source.GroupBy(keySelector).Select(a => new { Key = a.Key, Count = a.LongCount() }).ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        }

        public static IQueryable<TEntity> WhereSpecification<TEntity>(this IQueryable<TEntity> query, FilterSpecification<TEntity> specification) where TEntity : notnull
        {
            specification.Handle();

            return specification.SpecificationExpression == null ? query : query.Where(specification.SpecificationExpression);
        }
    }
}
