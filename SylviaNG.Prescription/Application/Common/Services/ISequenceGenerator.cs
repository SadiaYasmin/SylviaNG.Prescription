namespace SylviaNG.Prescription.Application.Common.Services
{
    /// <summary>
    /// Generates gap-free, monotonically increasing integers scoped to a
    /// (<paramref name="counterKey"/>, <paramref name="periodKey"/>) pair — e.g. a yearly
    /// display-code sequence ("ConsultationId", "2026") or a per-doctor, per-day token
    /// sequence ("ConsultationToken:{doctorId}", "2026-08-11"). Concurrency-safe: the
    /// Postgres implementation performs a single atomic upsert round trip rather than a
    /// read-then-write, so concurrent callers never observe or hand out the same value.
    /// </summary>
    public interface ISequenceGenerator
    {
        Task<int> GetNextAsync(string counterKey, string periodKey, CancellationToken cancellationToken = default);
    }
}
