using Flaminco.ManualMapper;

namespace WebApplication1.Mappers
{
    class SimpleMapHandler : IMapHandler<string, int>
    {
        public ValueTask<int> Handler(string source, Action<MapperOptions>? options = null, CancellationToken cancellationToken = default)
        {
            MapperOptions? mapperOptions = new MapperOptions();

            options?.Invoke(mapperOptions);

            return ValueTask.FromResult(int.Parse(mapperOptions?.Arguments["ReturnValue"]?.ToString()));
        }
    }
}
