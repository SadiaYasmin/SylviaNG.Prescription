using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Models
{
    /// <summary>
    /// The subset of a Patient needed to render the template's patient-info block
    /// (US-052) — not the full Patient CRUD shape from Epic B.
    /// </summary>
    public class PatientSnapshotResponse
    {
        public long PatientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public int? Age { get; set; }
        public GenderEnum? Gender { get; set; }
        public BloodGroupEnum? BloodGroup { get; set; }
        public AllergyPresetEnum? AllergyPresetId { get; set; }
        public string? AllergyOtherText { get; set; }
        public string? SavedHistory { get; set; }
    }
}
