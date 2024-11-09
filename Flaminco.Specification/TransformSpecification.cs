using System.Linq.Expressions;

namespace Flaminco.Specification
{
    public abstract class TransformSpecification<TEntity, TTarget> where TEntity : notnull
                                                                   where TTarget : notnull
    {
        internal Expression<Func<TEntity, TTarget>> SpecificationExpression { get; private set; } = default!;

        protected void Select(Expression<Func<TEntity, TTarget>> transform) => SpecificationExpression = transform;
    }
}
