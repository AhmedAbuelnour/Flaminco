using Flaminco.AuditTrail.Core.Tracker;

namespace Flaminco.AuditTrail.Core.Factory;

public class AuditTrailMapperFactory : IAuditTrailMapperFactory
{
    private readonly Func<IEnumerable<IAuditTrailMapper>> _factory;
    public AuditTrailMapperFactory(Func<IEnumerable<IAuditTrailMapper>> factory)
    {
        _factory = factory;
    }
    public IAuditTrailMapper? Create<T>()
    {
        return _factory()?.Where(a => a.GetType() == typeof(T))?.FirstOrDefault();
    }
}