namespace Flaminco.QueryableExtensions.Specifications
{
    public abstract class Specification<TEntity> where TEntity : notnull
    {
        public abstract IQueryable<TEntity> Where(IQueryable<TEntity> query);
    }

    public abstract class Specification<TEntity, TReturn> where TEntity : notnull where TReturn : notnull
    {
        public abstract IQueryable<TReturn> Select(IQueryable<TEntity> query);
    }

}
