using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Audit;

namespace SylviaNG.Prescription.Domain.Entities;

/// <summary>
/// A patient's clinical intake profile (Epic B). Unlike <see cref="Doctor"/>/<see cref="Staff"/>,
/// a Patient is NOT 1:1 with a <see cref="User"/> — patients never log in, have no Keycloak
/// account, and carry no UserId. <see cref="RegisteredByStaffId"/> records which Staff member
/// created the record and drives role-scoped visibility (see PatientVisibilityScope): a Staff
/// member sees only patients they registered, a Doctor sees patients registered by any Staff
/// member assigned to them via <see cref="StaffDoctor"/>, and Admin sees everyone.
/// </summary>
public class Patient : Audit
{
    public long PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public int? Age { get; set; }
    public GenderEnum? Gender { get; set; }
    public string? Address { get; set; }
    public BloodGroupEnum? BloodGroup { get; set; }
    public AllergyPresetEnum? AllergyPresetId { get; set; }
    public string? AllergyOtherText { get; set; }
    public string? SavedHistory { get; set; }
    public long RegisteredByStaffId { get; set; }
    public DateTime RegisteredAt { get; set; }
}
