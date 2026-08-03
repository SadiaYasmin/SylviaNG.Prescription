using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Auth.Models;
using SylviaNG.Prescription.Application.Interfaces.Externals;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Interfaces.Services;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SylviaNG.Prescription.Application.Services
{
    public class AuthService : IAuthService
    {
        private const string RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IKeycloakTokenClient _tokenClient;
        private readonly IKeycloakAdminClient _adminClient;

        public AuthService(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IKeycloakTokenClient tokenClient,
            IKeycloakAdminClient adminClient)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _tokenClient = tokenClient;
            _adminClient = adminClient;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var token = await _tokenClient.PasswordGrantAsync(request.Username, request.Password)
                ?? throw new InvalidCredentialsException();

            var claims = DecodeJwtClaims(token.AccessToken);
            var keycloakId = claims.GetValueOrDefault("sub")
                ?? throw new InvalidCredentialsException();
            var role = ExtractFirstRole(claims)
                ?? throw new InvalidCredentialsException();

            var user = await _userRepository.GetByKeycloakIdAsync(keycloakId);
            if (user == null)
            {
                // First login for an account that exists in Keycloak but has no local
                // profile row yet (e.g. the seeded dev users) — provision it now.
                user = new User
                {
                    KeycloakId = keycloakId,
                    Username = request.Username,
                    Role = role,
                    IsActive = true
                };
                await _userRepository.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }

            if (!user.IsActive)
                throw new InvalidCredentialsException();

            return new LoginResponse
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                ExpiresIn = token.ExpiresIn,
                Username = user.Username,
                Role = user.Role.ToString()
            };
        }

        public async Task<RefreshTokenResponse> RefreshAsync(string refreshToken)
        {
            var token = await _tokenClient.RefreshAsync(refreshToken)
                ?? throw new InvalidCredentialsException();

            return new RefreshTokenResponse
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                ExpiresIn = token.ExpiresIn
            };
        }

        public async Task LogoutAsync(string refreshToken)
        {
            await _tokenClient.LogoutAsync(refreshToken);
        }

        public async Task<CreateUserAccountResponse> CreateUserAccountAsync(CreateUserAccountRequest request)
        {
            var exists = await _userRepository.ExistsByUsernameAsync(request.Username);
            if (exists)
                throw new DuplicateException("User", "Username", request.Username);

            var temporaryPassword = GenerateTemporaryPassword();
            var firstName = string.IsNullOrWhiteSpace(request.FirstName) ? request.Username : request.FirstName;
            var lastName = string.IsNullOrWhiteSpace(request.LastName) ? request.Role.ToString() : request.LastName;
            // Keycloak's declarative User Profile requires email on this realm (same class of
            // "account not fully set up" trap as missing firstName/lastName) — always send one.
            var email = string.IsNullOrWhiteSpace(request.Email) ? $"{request.Username}@prescriptionms.local" : request.Email;
            var created = await _adminClient.CreateUserAsync(request.Username, email, firstName, lastName, temporaryPassword, request.Role.ToString());

            var user = new User
            {
                KeycloakId = created.KeycloakId,
                Username = request.Username,
                Email = email,
                Role = request.Role,
                IsActive = true
            };
            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return new CreateUserAccountResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                TemporaryPassword = temporaryPassword
            };
        }

        public async Task<ResetPasswordResponse> ResetPasswordAsync(long userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException("User", userId);

            var temporaryPassword = GenerateTemporaryPassword();
            await _adminClient.SetTemporaryPasswordAsync(user.KeycloakId, temporaryPassword);

            return new ResetPasswordResponse { TemporaryPassword = temporaryPassword };
        }

        private static string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";
            var bytes = RandomNumberGenerator.GetBytes(16);
            var builder = new StringBuilder(16);
            foreach (var b in bytes)
                builder.Append(chars[b % chars.Length]);
            return builder.ToString();
        }

        private static Dictionary<string, string> DecodeJwtClaims(string accessToken)
        {
            var parts = accessToken.Split('.');
            if (parts.Length < 2)
                throw new InvalidCredentialsException();

            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            using var document = JsonDocument.Parse(payloadJson);

            var claims = new Dictionary<string, string>();
            foreach (var property in document.RootElement.EnumerateObject())
            {
                claims[property.Name] = property.Value.ValueKind == JsonValueKind.Array
                    ? property.Value.ToString()
                    : property.Value.ToString();
            }
            return claims;
        }

        private static UserRoleEnum? ExtractFirstRole(Dictionary<string, string> claims)
        {
            if (!claims.TryGetValue(RoleClaimType, out var raw))
                return null;

            using var document = JsonDocument.Parse(raw.StartsWith('[') ? raw : $"[{JsonSerializer.Serialize(raw)}]");
            foreach (var item in document.RootElement.EnumerateArray())
            {
                if (Enum.TryParse<UserRoleEnum>(item.GetString(), ignoreCase: true, out var role))
                    return role;
            }
            return null;
        }

        private static byte[] Base64UrlDecode(string input)
        {
            var padded = input.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }
            return Convert.FromBase64String(padded);
        }
    }
}
