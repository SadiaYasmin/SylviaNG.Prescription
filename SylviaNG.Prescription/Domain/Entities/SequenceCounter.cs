namespace SylviaNG.Prescription.Domain.Entities;

/// <summary>
/// Backing row for <see cref="Application.Common.Services.ISequenceGenerator"/> (Epic C).
/// One row per (CounterKey, PeriodKey) pair — e.g. CounterKey "ConsultationId" with
/// PeriodKey "2026" for a yearly display-code sequence, or CounterKey
/// "ConsultationToken:{doctorId}" with PeriodKey "2026-08-11" for a per-doctor,
/// per-day token sequence. Deliberately NOT an <see cref="SharedKernel.Audit.Audit"/>
/// subclass — this is a pure technical counter, not a business/tenant-scoped record.
/// </summary>
public class SequenceCounter
{
    public long Id { get; set; }
    public string CounterKey { get; set; } = string.Empty;
    public string PeriodKey { get; set; } = string.Empty;
    public int LastValue { get; set; }
}
