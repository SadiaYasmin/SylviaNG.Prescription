using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Audit;

namespace SylviaNG.Prescription.Domain.Entities;

/// <summary>
/// A single patient's queued/attended visit with a specific doctor on a specific day
/// (Epic C). <see cref="DisplayCode"/> is a globally unique, human-facing identifier
/// (e.g. "CN-2026-0001"); <see cref="TokenNumber"/> is the per-doctor, per-day queue
/// position shown at check-in (e.g. "T-03") — both are minted via
/// <see cref="Application.Common.Services.ISequenceGenerator"/>, never guessed/incremented
/// in application code. <see cref="CheckInAt"/> is set explicitly by the handler
/// (<c>DateTime.UtcNow</c>) rather than relying on <see cref="Audit.CreatedAt"/>, which is
/// never auto-populated anywhere in this codebase (UtcDateTimeInterceptor only normalizes
/// timezones, it doesn't stamp CreatedAt).
/// </summary>
public class Consultation : Audit
{
    public long ConsultationId { get; set; }
    public string DisplayCode { get; set; } = string.Empty;
    public long PatientId { get; set; }
    public long DoctorId { get; set; }
    public long RegisteredByStaffId { get; set; }
    public DateOnly VisitDate { get; set; }
    public string TokenNumber { get; set; } = string.Empty;

    /// <summary>
    /// The consultation's own lifecycle status. Deliberately shadows (via <c>new</c>)
    /// <see cref="Audit.Status"/> — an unrelated generic int field on the shared Audit base
    /// that this entity has no use for — rather than picking an awkward alternate name just
    /// to dodge the collision; EF maps this most-derived declaration to the "Status" column.
    /// </summary>
    public new ConsultationStatusEnum Status { get; set; }

    public DateTime CheckInAt { get; set; }
}
