using Flaminco.AuditTrail.Core.Tracker;

namespace Flaminco.AuditTrail.Core.Factory;

public interface IAuditTrailMapperFactory
{
    IAuditTrailMapper? Create<T>();
}