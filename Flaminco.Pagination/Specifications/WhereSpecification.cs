using Flaminco.QueryableExtensions.Extensions;
using System.Linq.Expressions;

namespace Flaminco.QueryableExtensions.Specifications
{
    public abstract class WhereSpecification<TSource> where TSource : notnull
    {
        internal Expression<Func<TSource, bool>> SpecificationExpression { get; private set; } = default!;

        protected void Where(Expression<Func<TSource, bool>> filter)
        {
            SpecificationExpression = SpecificationExpression == null ? filter : CombineExpressions(SpecificationExpression, filter);
        }

        protected void MultiColumnSearch(string filter, params Expression<Func<TSource, string?>>[] properties)
        {
            if (FilterExtensions.MultiColumnSearch(filter, properties) is Expression<Func<TSource, bool>> buildExpression)
            {
                SpecificationExpression = SpecificationExpression == null ? buildExpression : CombineExpressions(SpecificationExpression, buildExpression);
            }
        }

        public abstract void Handle();

        // Helper method to combine expressions with AND
        private static Expression<Func<TSource, bool>> CombineExpressions(Expression<Func<TSource, bool>> first, Expression<Func<TSource, bool>> second)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(TSource));

            BinaryExpression body = Expression.AndAlso(
                new ReplaceParameterVisitor(first.Parameters[0], parameter).Visit(first.Body),
                new ReplaceParameterVisitor(second.Parameters[0], parameter).Visit(second.Body));

            return Expression.Lambda<Func<TSource, bool>>(body, parameter);
        }

        private class ReplaceParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter) : ExpressionVisitor
        {
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == oldParameter ? newParameter : base.VisitParameter(node);
            }
        }
    }
}
