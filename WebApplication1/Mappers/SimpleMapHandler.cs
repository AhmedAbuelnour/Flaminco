using Flaminco.ManualMapper.Abstractions;

namespace WebApplication1.Mappers
{
    public record SimpleMapProfile(string Name) : IMapProfile<List<int>>;
    public record SimpleMap2Profile(string Name) : IMapProfile<List<int>>;


    public class SimpleMapHandler : IMapProfileHandler<SimpleMapProfile, List<int>>
    {

        public ValueTask<List<int>> Handler(SimpleMapProfile profile, CancellationToken cancellationToken = default)
        {
            if (profile.Name == "Ahmed")
            {

                return ValueTask.FromResult(new List<int> { 1994, 12, 14 });
            }
            return ValueTask.FromResult(new List<int> { 15, 16, 17 });
        }
    }

    public class SimpleMapHandler2 : IMapProfileHandler<SimpleMap2Profile, List<int>>
    {

        public ValueTask<List<int>> Handler(SimpleMap2Profile profile, CancellationToken cancellationToken = default)
        {
            if (profile.Name == "Ahmed")
            {

                return ValueTask.FromResult(new List<int> { 1995, 12, 14 });
            }
            return ValueTask.FromResult(new List<int> { 15, 16, 17 });
        }
    }
}
