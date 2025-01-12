using Flaminco.QueryableExtensions.Specifications;

namespace Flaminco.QueryableExtensions.Extensions
{
    public static class SpecificationExtensions
    {
        public static IQueryable<TReturn> WithSpecification<TEntity, TReturn, TSpecification>(this IQueryable<TEntity> query)
            where TEntity : notnull
            where TReturn : notnull
            where TSpecification : Specification<TEntity, TReturn>, new()
            => new TSpecification().Select(query);

        public static IQueryable<TReturn> WithSpecification<TEntity, TReturn>(this IQueryable<TEntity> query, Specification<TEntity, TReturn> specification)
            where TEntity : notnull
            where TReturn : notnull
            => specification.Select(query);


        public static IQueryable<TEntity> WithSpecification<TEntity>(this IQueryable<TEntity> query, Specification<TEntity> specification) where TEntity : notnull
            => specification.Where(query);
    }
}
