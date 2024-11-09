using System.Linq.Expressions;

namespace Flaminco.QueryableExtensions.Specifications
{
    public abstract class FilterSpecification<TEntity> where TEntity : notnull
    {
        internal Expression<Func<TEntity, bool>> SpecificationExpression { get; private set; } = default!;

        protected void Where(Expression<Func<TEntity, bool>> filter)
        {
            SpecificationExpression = SpecificationExpression == null ? filter : CombineExpressions(SpecificationExpression, filter);
        }

        public abstract void Handle();

        // Helper method to combine expressions with AND
        private static Expression<Func<TEntity, bool>> CombineExpressions(Expression<Func<TEntity, bool>> first, Expression<Func<TEntity, bool>> second)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(TEntity));

            BinaryExpression body = Expression.AndAlso(
                new ReplaceParameterVisitor(first.Parameters[0], parameter).Visit(first.Body),
                new ReplaceParameterVisitor(second.Parameters[0], parameter).Visit(second.Body));

            return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
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
