using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Doctors.Models
{
    public class CreateDoctorRequest
    {
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Qualification { get; set; }
        public string? Department { get; set; }
        public string? LicenseNumber { get; set; }
        public string? Specialization { get; set; }
        public int? ExperienceYears { get; set; }
        public GenderEnum? Gender { get; set; }
        public DateOnly? JoiningDate { get; set; }
        public string? PhotoBase64 { get; set; }
    }
}
