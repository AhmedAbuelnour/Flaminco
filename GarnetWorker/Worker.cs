using Flaminco.Garnet.Server.Background;

namespace GarnetWorker
{
    public class Worker : GarnetServerBackgroundServiceBase
    {
        public override string ConfigurationPath => throw new NotImplementedException();

    }
}
