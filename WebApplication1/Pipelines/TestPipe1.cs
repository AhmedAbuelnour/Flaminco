using Flaminco.Pipeline.Abstractions;
using Flaminco.Pipeline.Attributes;

namespace WebApplication1.Pipelines
{
    public class Demo
    {
        public int Value { get; set; }
    }
    [Pipeline(Name = "TEST", Order = 2)]
    public class TestPipe1 : IPipelineHandler<Demo>
    {
        public ValueTask Handler(Demo source, CancellationToken cancellationToken = default)
        {
            source.Value = 1;

            return ValueTask.CompletedTask;
        }
    }

    [Pipeline(Name = "TEST", Order = 1)]
    public class TestPipe2 : IPipelineHandler<Demo>
    {
        public ValueTask Handler(Demo source, CancellationToken cancellationToken = default)
        {
            source.Value = 2;

            return ValueTask.CompletedTask;
        }
    }
}
