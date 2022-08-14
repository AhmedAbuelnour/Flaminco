using Flaminco.ManualMapper;

namespace WebApplication1.Mappers
{
    public record SimpleMapProfile(string Name) : IMapProfile<int>;


    public class SimpleMapHandler : IMapProfileHandler<SimpleMapProfile, int>
    {
        public ValueTask<int> Handler(SimpleMapProfile profile, Action<MapperOptions>? options = null, CancellationToken cancellationToken = default)
        {
            MapperOptions? mapperOptions = new MapperOptions();

            options?.Invoke(mapperOptions);

            if (profile.Name == "Ahmed")
            {
                return ValueTask.FromResult(15);
            }
            return ValueTask.FromResult(int.Parse(mapperOptions?.Arguments["ReturnValue"]?.ToString()));
        }
    }
}
