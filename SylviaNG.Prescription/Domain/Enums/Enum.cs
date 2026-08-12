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
/// A consultation's lifecycle state (Epic C). Deliberately only these three members —
/// no "Draft"/"Cancelled" — those are out of scope until a later story needs them.
/// </summary>
public enum ConsultationStatusEnum
{
    Waiting,
    InConsultation,
    Completed
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
