namespace Flaminco.AuditTrail.Core.Tracker;

public interface IAuditTrailMapper
{
    ValueTask<TIntermediate> Map<TSource, TIntermediate>(TSource source);
}