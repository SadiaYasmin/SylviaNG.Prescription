namespace SylviaNG.Prescription.Domain.Enums;

public enum UserRoleEnum
{
    Admin,
    Doctor,
    Staff
}

public enum GenderEnum
{
    Male,
    Female,
    Other
}

public enum BloodGroupEnum
{
    APositive,
    ANegative,
    BPositive,
    BNegative,
    ABPositive,
    ABNegative,
    OPositive,
    ONegative
}

/// <summary>
/// The fixed 5-item allergy preset list (Epic B). "Other" is intentionally NOT a member
/// here — it's represented by <c>AllergyPresetId == null</c> combined with a non-null
/// <c>AllergyOtherText</c> free-text value on <see cref="Entities.Patient"/>.
/// </summary>
public enum AllergyPresetEnum
{
    None = 1,
    Penicillin = 2,
    Dust = 3,
    Seafood = 4,
    Latex = 5
}

/// <summary>
/// A consultation's lifecycle state (Epic C, extended by Epic D). <c>Draft</c> was added
/// once <see cref="Entities.Prescription"/> existed to need it (a consultation whose
/// prescription was saved as a draft, taken out of the live queue) — <c>Cancelled</c>
/// remains out of scope, no story needs it yet.
/// </summary>
public enum ConsultationStatusEnum
{
    Waiting,
    InConsultation,
    Completed,
    Draft
}

/// <summary>
/// A prescription's lifecycle state (Epic D).
/// <list type="bullet">
/// <item><c>InProgress</c> — actively being authored. The row is created in this state the
/// moment authoring starts and is continuously auto-saved for data protection, but is
/// deliberately kept OUT of the Draft Prescriptions list. It becomes <c>Draft</c> only when
/// the doctor leaves the unfinished prescription or explicitly saves it as a draft.</item>
/// <item><c>Draft</c> — parked/saved and shown in the Draft Prescriptions list.</item>
/// <item><c>Finalized</c> — explicitly finalized; permanently read-only.</item>
/// </list>
/// <c>InProgress</c> is appended last (value 2) on purpose — inserting it ahead of the
/// existing members would renumber <c>Draft</c>/<c>Finalized</c> and mis-map already-stored
/// integer status values.
/// </summary>
public enum PrescriptionStatusEnum
{
    Draft,
    Finalized,
    InProgress
}

/// <summary>
/// Which authoring section a doctor's saved Quick Add preset (Epic G stub) applies to.
/// One shared <see cref="Entities.QuickAddPreset"/> table covers all five kinds.
/// </summary>
public enum QuickAddSectionTypeEnum
{
    Medicine,
    Diagnosis,
    Investigation,
    Advice,
    FollowUp
}

/// <summary>
/// Which date-range mode GetConsultationList's admin listing filters by (Epic C).
/// Custom uses <c>Date</c>; Range uses <c>FromDate</c>/<c>ToDate</c>.
/// </summary>
public enum ConsultationDateModeEnum
{
    Today,
    Yesterday,
    Custom,
    Range
}

/// <summary>
/// Which slice of a doctor's staff-scoped patient roster GetDoctorPatientQueue returns
/// (Create Prescription's "Patient Filter"). <c>NotConsultedToday</c> is deliberately the
/// inclusive complement of <c>CompletedToday</c> — it covers both patients with no
/// consultation record yet today AND patients currently Waiting/InConsultation, not just
/// patients who haven't checked in at all.
/// </summary>
public enum PatientQueueFilterEnum
{
    TodayQueue,
    AllRegistered,
    NotConsultedToday,
    CompletedToday
}
