namespace Flaminco.QueryableExtensions.Specifications
{
    public abstract class WhereSpecification<TSource> where TSource : notnull
    {
        public abstract IQueryable<TSource> Where(IQueryable<TSource> query);
    }
}
