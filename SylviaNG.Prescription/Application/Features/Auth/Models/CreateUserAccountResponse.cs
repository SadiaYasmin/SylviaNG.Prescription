namespace SylviaNG.Prescription.Application.Features.Auth.Models
{
    public class CreateUserAccountResponse
    {
        public long UserId { get; set; }
        public string Username { get; set; } = string.Empty;
    }
}
