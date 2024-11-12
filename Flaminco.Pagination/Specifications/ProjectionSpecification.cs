namespace Flaminco.QueryableExtensions.Specifications
{
    public abstract class ProjectionSpecification<TSource, TProject> where TSource : notnull
                                                                    where TProject : notnull
    {
        public abstract IQueryable<TProject> ProjectTo(IQueryable<TSource> query);
    }
}
