using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Audit;

namespace SylviaNG.Prescription.Domain.Entities;

/// <summary>
/// The live, inline-authored clinical document (Epic D/E). Named <c>PrescriptionRecord</c>,
/// not the bare "Prescription" the rest of the domain uses in routes/JSON/docs, because the
/// project's own root namespace is <c>SylviaNG.Prescription</c> — an entity literally named
/// <c>Prescription</c> is unresolvable (CS0118, namespace-vs-type ambiguity) anywhere in this
/// codebase. A row is created (empty, <see cref="PrescriptionStatusEnum.Draft"/>) the moment
/// authoring starts — whether by opening a queued <see cref="Consultation"/> or a
/// quick-create walk-in — so every draft has a stable <see cref="DisplayCode"/> from the
/// first keystroke; "Save as Draft" just updates the same row. <see cref="ConsultationId"/>
/// is a unique 1:1 FK: a PrescriptionRecord never exists without exactly one Consultation
/// (quick-create silently creates one), which is what keeps the consultation/prescription
/// status invariant (US-017) simple to enforce transactionally. <see cref="TemplateId"/> is
/// snapshotted at creation from the doctor's <see cref="Doctor.PreferredTemplateId"/> at that
/// moment — never re-resolved later (US-064). List-type sections are serialized JSON text
/// columns (same portable-across-providers pattern as
/// <see cref="PrescriptionTemplate.ConfigJson"/>, mapped via
/// Application/Mappings/PrescriptionMappings.cs), not EF owned-entity `.ToJson()`.
/// Examination/vitals are discrete nullable string columns instead, since that shape is
/// fixed and known (matches the reference prototype, where even BP is free text like
/// "120/80", not split systolic/diastolic).
/// </summary>
public class PrescriptionRecord : Audit
{
    public long PrescriptionId { get; set; }
    public string DisplayCode { get; set; } = string.Empty;
    public long ConsultationId { get; set; }
    public long PatientId { get; set; }
    public long DoctorId { get; set; }
    public long TemplateId { get; set; }
    public TemplateLanguageEnum Language { get; set; }
    public PrescriptionStatusEnum Status { get; set; }

    public string ChiefComplaintsJson { get; set; } = "[]";
    public string HistoryJson { get; set; } = "[]";
    public string DiagnosesJson { get; set; } = "[]";
    public string InvestigationsJson { get; set; } = "[]";
    public string MedicinesJson { get; set; } = "[]";
    public string AdviceJson { get; set; } = "[]";

    public string? Bp { get; set; }
    public string? Pulse { get; set; }
    public string? Temperature { get; set; }
    public string? RespiratoryRate { get; set; }
    public string? Spo2 { get; set; }
    public string? Weight { get; set; }
    public string? Height { get; set; }
    public string? BloodSugar { get; set; }
    public string? PainScore { get; set; }
    public string? HeartRate { get; set; }

    public string? FollowUp { get; set; }

    public DateTime? SavedAt { get; set; }
    public DateTime? FinalizedAt { get; set; }
}
