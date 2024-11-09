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
    }
}
