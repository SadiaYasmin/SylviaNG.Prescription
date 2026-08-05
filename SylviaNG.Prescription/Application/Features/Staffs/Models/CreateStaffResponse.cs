namespace SylviaNG.Prescription.Application.Features.Staffs.Models
{
    public class CreateStaffResponse
    {
        public long StaffId { get; set; }
        public long UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string TemporaryPassword { get; set; } = string.Empty;
    }
}
