using SylviaNG.Prescription.Application.Features.Auth.Models;

namespace SylviaNG.Prescription.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<RefreshTokenResponse> RefreshAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
        Task<CreateUserAccountResponse> CreateUserAccountAsync(CreateUserAccountRequest request);
        Task<ResetPasswordResponse> ResetPasswordAsync(long userId);
    }
}
