namespace SylviaNG.Prescription.Application.Features.Doctors.Models
{
    /// <summary>
    /// Epic K (US-061): a doctor's own self-editable profile — deliberately a strict subset
    /// of <see cref="DoctorSummaryResponse"/>. Specialization/ExperienceYears/Gender/
    /// JoiningDate/IsActive stay Admin-only edits via DoctorsController; this surface never
    /// exposes or accepts them.
    /// </summary>
    public class DoctorProfileResponse
    {
        public long DoctorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Qualification { get; set; }
        public string? Department { get; set; }
        public string? LicenseNumber { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhotoBase64 { get; set; }
    }

    public class UpdateDoctorProfileRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string? Qualification { get; set; }
        public string? Department { get; set; }
        public string? LicenseNumber { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
    }

    public class UpdateDoctorPhotoRequest
    {
        public string? PhotoBase64 { get; set; }
    }
}
