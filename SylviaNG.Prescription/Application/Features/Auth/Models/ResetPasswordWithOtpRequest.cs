namespace SylviaNG.Prescription.Application.Features.Auth.Models
{
    public class ResetPasswordWithOtpRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
