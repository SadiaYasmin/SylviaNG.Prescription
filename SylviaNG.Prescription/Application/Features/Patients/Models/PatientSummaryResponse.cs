using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Patients.Models
{
    public class PatientSummaryResponse
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

        /// <summary>
        /// Always populated regardless of caller role — access control is which ROWS come
        /// back (see PatientVisibilityScope), not which fields; the frontend chooses to
        /// only display this for Admin.
        /// </summary>
        public string RegisteredByName { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
    }
}
