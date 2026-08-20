using SylviaNG.Prescription.Application.Features.Auth.Models;

namespace SylviaNG.Prescription.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<RefreshTokenResponse> RefreshAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);

        /// <summary>Creates the account with no password — the invitee sets their own via the emailed Keycloak link.</summary>
        Task<CreateUserAccountResponse> CreateUserAccountAsync(CreateUserAccountRequest request);

        /// <summary>Admin-forced reset — re-sends the same "set your password" invite email rather than issuing a visible temp password.</summary>
        Task ResetPasswordAsync(long userId);

        // Forgot password (anonymous, OTP-code based, generic responses — never discloses
        // whether an email is registered).
        Task RequestPasswordResetOtpAsync(string email);
        Task<bool> VerifyPasswordResetOtpAsync(string email, string code);
        Task ResetPasswordWithOtpAsync(string email, string code, string newPassword);

        // Self-service change email/password (logged-in, verify-before-apply).
        Task RequestEmailChangeAsync(string keycloakId, string newEmail);
        Task ConfirmEmailChangeAsync(string keycloakId, string code);
        Task RequestPasswordChangeAsync(string keycloakId);
        Task ConfirmPasswordChangeAsync(string keycloakId, string code, string newPassword);
    }
}
