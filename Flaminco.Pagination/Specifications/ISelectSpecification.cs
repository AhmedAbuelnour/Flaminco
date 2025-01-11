namespace Flaminco.QueryableExtensions.Specifications
{
    public interface ISelectSpecification<TSource, TProject> where TSource : notnull
                                                             where TProject : notnull
    {
        IQueryable<TProject> Select(IQueryable<TSource> query);
    }
}
