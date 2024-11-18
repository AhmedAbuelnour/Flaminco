using Flaminco.QueryableExtensions.Specifications;

namespace Flaminco.QueryableExtensions.Extensions
{
    public static class SpecificationExtensions
    {
        public static IQueryable<TProject> ProjectSpecification<TSource, TProject, TSpecification>(this IQueryable<TSource> query)
            where TSource : notnull
            where TProject : notnull
            where TSpecification : ProjectSpecification<TSource, TProject>, new()
        {
            return new TSpecification().ProjectTo(query);
        }

        public static IQueryable<TProject> ProjectSpecification<TSource, TProject>(this IQueryable<TSource> query, ProjectSpecification<TSource, TProject> projectionSpecification)
        where TSource : notnull
        where TProject : notnull
        {
            return projectionSpecification.ProjectTo(query);
        }


        public static IQueryable<TSource> WhereSpecification<TSource>(this IQueryable<TSource> query, WhereSpecification<TSource> specification) where TSource : notnull
        {
            specification.Handle();

            return specification.SpecificationExpression == null ? query : query.Where(specification.SpecificationExpression);
        }
    }
}
