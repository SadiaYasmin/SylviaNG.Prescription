using SylviaNG.Prescription.Application.Common.Models;

namespace SylviaNG.Prescription.Application.Interfaces.Externals
{
    public interface IKeycloakAdminClient
    {
        /// <summary>
        /// Creates the Keycloak user with no credentials at all — the account is
        /// activated by the invite email (<see cref="SendRequiredActionsEmailAsync"/>),
        /// never a password issued by this backend.
        /// </summary>
        Task<KeycloakCreatedUser> CreateUserAsync(string username, string email, string firstName, string lastName, string realmRole);
        Task SetTemporaryPasswordAsync(string keycloakId, string newPassword);
        Task SetUserEnabledAsync(string keycloakId, bool enabled);
        Task SetUserEmailAsync(string keycloakId, string email);

        /// <summary>
        /// Triggers Keycloak's own "required actions" email (e.g. account-created /
        /// set-your-password, or re-verify email) — sent via the realm's configured SMTP
        /// server, not this backend's <c>IEmailService</c>.
        /// </summary>
        Task SendRequiredActionsEmailAsync(string keycloakId, IEnumerable<string> actions);
    }
}
