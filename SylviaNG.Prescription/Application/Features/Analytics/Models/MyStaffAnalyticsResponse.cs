namespace SylviaNG.Prescription.Application.Features.Analytics.Models
{
    public class AssignedDoctorEntry
    {
        public long DoctorId { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

    /// <summary>US-078: a staff member's own scoped stats.</summary>
    public class MyStaffAnalyticsResponse
    {
        public int PatientsRegisteredByMe { get; set; }
        public List<AssignedDoctorEntry> AssignedDoctors { get; set; } = new();
    }
}
