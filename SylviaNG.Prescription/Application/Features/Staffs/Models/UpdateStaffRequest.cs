namespace SylviaNG.Prescription.Application.Features.Staffs.Models
{
    public class UpdateStaffRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? Department { get; set; }
        public List<long> AssignedDoctorIds { get; set; } = new();
        public bool IsActive { get; set; } = true;
    }
}
