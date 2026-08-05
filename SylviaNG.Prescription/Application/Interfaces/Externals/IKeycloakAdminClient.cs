using SylviaNG.Prescription.Application.Common.Models;

namespace SylviaNG.Prescription.Application.Interfaces.Externals
{
    public interface IKeycloakAdminClient
    {
        Task<KeycloakCreatedUser> CreateUserAsync(string username, string? email, string firstName, string lastName, string temporaryPassword, string realmRole);
        Task SetTemporaryPasswordAsync(string keycloakId, string newPassword);
        Task SetUserEnabledAsync(string keycloakId, bool enabled);
    }
}
