namespace Flaminco.ManualMapper
{
    public class ManualMapper : IManualMapper
    {
        public ValueTask<TDestination> Map<TSource, TDestination>(IMapHandler<TSource, TDestination> handler, TSource source, Action<MapperOptions>? options = null, CancellationToken cancellationToken = default)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }
            return handler.Handler(source, options, cancellationToken);
        }
    }
}
