namespace Flaminco.ManualMapper;

public class MapperOptions
{
    public MapperOptions()
    {
        Arguments = new Dictionary<string, object>();
    }
    public IDictionary<string, object>? Arguments { get; set; }
}
