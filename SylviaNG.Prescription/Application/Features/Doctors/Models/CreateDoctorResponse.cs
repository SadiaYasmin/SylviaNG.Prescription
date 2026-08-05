namespace SylviaNG.Prescription.Application.Features.Doctors.Models
{
    public class CreateDoctorResponse
    {
        public long DoctorId { get; set; }
        public long UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string TemporaryPassword { get; set; } = string.Empty;
    }
}
