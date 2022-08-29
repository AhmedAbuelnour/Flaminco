using Flaminco.ManualMapper;

namespace WebApplication1.Mappers
{
    public record SimpleMapProfile(string Name) : IMapProfile<List<int>>;


    public class SimpleMapHandler : IMapProfileHandler<SimpleMapProfile, List<int>>
    {
        public ValueTask<List<int>> Handler(SimpleMapProfile profile, Action<MapperOptions>? options = null, CancellationToken cancellationToken = default)
        {
            MapperOptions? mapperOptions = new MapperOptions();

            options?.Invoke(mapperOptions);

            if (profile.Name == "Ahmed")
            {
                return ValueTask.FromResult(new List<int> { 15, 16 });
            }
            return ValueTask.FromResult(new List<int> { 15, 16, 17 });
        }
    }
}
