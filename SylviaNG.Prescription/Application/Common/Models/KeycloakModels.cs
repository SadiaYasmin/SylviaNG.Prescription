namespace SylviaNG.Prescription.Application.Common.Models
{
    public class KeycloakTokenResult
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
    }

    public class KeycloakCreatedUser
    {
        public string KeycloakId { get; set; } = string.Empty;
    }
}
