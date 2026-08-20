namespace SylviaNG.Prescription.Application.Features.Auth.Models
{
    public class CurrentUserResponse
    {
        public long UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
