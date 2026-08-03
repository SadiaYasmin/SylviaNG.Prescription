using SylviaNG.Prescription.Application.Common.Models;

namespace SylviaNG.Prescription.Application.Interfaces.Externals
{
    public interface IKeycloakTokenClient
    {
        Task<KeycloakTokenResult?> PasswordGrantAsync(string username, string password);
        Task<KeycloakTokenResult?> RefreshAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
    }
}
