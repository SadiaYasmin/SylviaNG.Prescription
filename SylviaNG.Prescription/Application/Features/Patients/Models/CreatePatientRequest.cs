using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Patients.Models
{
    public class CreatePatientRequest
    {
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
    }
}
