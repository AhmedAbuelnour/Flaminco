using Flaminco.QueryableExtensions.Specifications;

namespace Flaminco.QueryableExtensions.Extensions
{
    public static class SpecificationExtensions
    {
        public static IQueryable<TOutput> SelectSpecification<TEntity, TOutput, TSpecification>(this IQueryable<TEntity> query)
            where TEntity : notnull
            where TOutput : notnull
            where TSpecification : ISelectSpecification<TEntity, TOutput>, new()
            => new TSpecification().Select(query);

        public static IQueryable<TOutput> SelectSpecification<TEntity, TOutput>(this IQueryable<TEntity> query, ISelectSpecification<TEntity, TOutput> specification)
            where TEntity : notnull
            where TOutput : notnull
            => specification.Select(query);


        public static IQueryable<TEntity> WhereSpecification<TEntity>(this IQueryable<TEntity> query, IWhereSpecification<TEntity> specification) where TEntity : notnull
            => specification.Where(query);
    }
}
