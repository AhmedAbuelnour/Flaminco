using Flaminco.QueryableExtensions.Specifications;

namespace Flaminco.QueryableExtensions.Extensions
{
    public static class ProjectionExtensions
    {
        public static IQueryable<TProject> ProjectSpecification<TSource, TProject, TSpecification>(this IQueryable<TSource> query)
            where TSource : notnull
            where TProject : notnull
            where TSpecification : ProjectionSpecification<TSource, TProject>, new()
        {
            return new TSpecification().ProjectTo(query);
        }

        public static IQueryable<TProject> ProjectSpecification<TSource, TProject>(this IQueryable<TSource> query, ProjectionSpecification<TSource, TProject> projectionSpecification)
        where TSource : notnull
        where TProject : notnull
        {
            return projectionSpecification.ProjectTo(query);
        }
    }
}
