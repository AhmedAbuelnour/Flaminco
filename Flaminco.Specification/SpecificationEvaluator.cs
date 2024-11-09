namespace Flaminco.Specification
{
    public static class SpecificationEvaluator
    {
        public static IQueryable<TEntity> ApplyFilterSpecification<TEntity>(this IQueryable<TEntity> query, FilterSpecification<TEntity> specification) where TEntity : notnull
        {
            specification.Handle();

            return specification.SpecificationExpression == null ? query : query.Where(specification.SpecificationExpression);
        }

        public static IQueryable<TTarget> ApplyTransformSpecification<TEntity, TTarget>(this IQueryable<TEntity> query, TransformSpecification<TEntity, TTarget> specification) where TEntity : notnull where TTarget : notnull
        {
            if (specification.SpecificationExpression == null)
            {
                throw new InvalidOperationException("TransformSpecification's SpecificationExpression is not set.");
            }

            return query.Select(specification.SpecificationExpression);
        }
    }
}
