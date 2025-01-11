namespace Flaminco.QueryableExtensions.Specifications
{
    public interface IWhereSpecification<TSource> where TSource : notnull
    {
        IQueryable<TSource> Where(IQueryable<TSource> query);
    }
}
