namespace SylviaNG.Prescription.Application.Features.Staffs.Models
{
    public class StaffSummaryResponse
    {
        public long StaffId { get; set; }
        public long UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Phone { get; set; } = string.Empty;

        /// <summary>Derived, deduped from the assigned doctors' Department — see StaffMappings.</summary>
        public List<string> Departments { get; set; } = new();
        public bool IsActive { get; set; }
        public List<AssignedDoctorSummary> AssignedDoctors { get; set; } = new();
    }
}
