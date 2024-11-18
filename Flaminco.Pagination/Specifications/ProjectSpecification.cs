namespace Flaminco.QueryableExtensions.Specifications
{
    public abstract class ProjectSpecification<TSource, TProject> where TSource : notnull
                                                                    where TProject : notnull
    {
        public abstract IQueryable<TProject> ProjectTo(IQueryable<TSource> query);
    }
}
