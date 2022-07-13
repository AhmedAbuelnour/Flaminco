namespace Flaminco.AuditTrail.Core.Tracker;

public interface IAuditTrailTracker<TIntermediate, TSource> where TIntermediate : ITracker
{
    ValueTask<Guid> SetTrackerAsync<TMapper>(TSource source, string primaryKeyValue, string userId, CancellationToken cancellationToken);
    ValueTask GetTrackerResultAsync<TMapper>(TSource source, ActionType actionType, Action<AuditSnapshot> onSnapshotGenerate, CancellationToken cancellationToken);
}