namespace SylviaNG.Prescription.Application.Features.Staffs.Models
{
    public class CreateStaffRequest
    {
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public List<long> AssignedDoctorIds { get; set; } = new();
    }
}
