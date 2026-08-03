namespace SylviaNG.Prescription.Application.Features.Auth.Models
{
    public class LogoutRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
