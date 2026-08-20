using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Auth.Models;
using SylviaNG.Prescription.Application.Interfaces.Externals;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Interfaces.Services;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SylviaNG.Prescription.Application.Services
{
    public class AuthService : IAuthService
    {
        private const string RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
        private static readonly TimeSpan OtpValidity = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan OtpResendCooldown = TimeSpan.FromSeconds(60);
        private const int OtpMaxAttempts = 5;

        private readonly IUserRepository _userRepository;
        private readonly IVerificationCodeRepository _verificationCodeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IKeycloakTokenClient _tokenClient;
        private readonly IKeycloakAdminClient _adminClient;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IVerificationCodeRepository verificationCodeRepository,
            IUnitOfWork unitOfWork,
            IKeycloakTokenClient tokenClient,
            IKeycloakAdminClient adminClient,
            IEmailService emailService,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _verificationCodeRepository = verificationCodeRepository;
            _unitOfWork = unitOfWork;
            _tokenClient = tokenClient;
            _adminClient = adminClient;
            _emailService = emailService;
            _logger = logger;
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

            var firstName = string.IsNullOrWhiteSpace(request.FirstName) ? request.Username : request.FirstName;
            var lastName = string.IsNullOrWhiteSpace(request.LastName) ? request.Role.ToString() : request.LastName;
            var created = await _adminClient.CreateUserAsync(request.Username, request.Email, firstName, lastName, request.Role.ToString());

            // Invite email: Keycloak's own hosted "set your password" + "verify your email"
            // flow (realm SMTP config) — this backend never generates or sees a password for
            // an account it creates. Deliberately non-fatal: if the send fails (bad SMTP
            // config, provider outage, etc.), the account must still be created — otherwise
            // the Keycloak user above is already committed with no compensating rollback,
            // leaving an orphaned Keycloak user that permanently 409-conflicts on retry with
            // the same username/email, and no way to reach it since it was never persisted
            // here. "Resend Account Setup Email" exists precisely so a failed send here isn't
            // fatal — an admin can retry it once the underlying mail issue is fixed.
            try
            {
                await _adminClient.SendRequiredActionsEmailAsync(created.KeycloakId, new[] { "UPDATE_PASSWORD", "VERIFY_EMAIL" });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Account setup invite email failed to send for new user {Username} ({KeycloakId}). The account was still created — use Resend Account Setup Email once mail delivery is fixed.", request.Username, created.KeycloakId);
            }

            var user = new User
            {
                KeycloakId = created.KeycloakId,
                Username = request.Username,
                Email = request.Email,
                Role = request.Role,
                IsActive = true
            };
            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return new CreateUserAccountResponse
            {
                UserId = user.UserId,
                Username = user.Username
            };
        }

        public async Task ResetPasswordAsync(long userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException("User", userId);

            await _adminClient.SendRequiredActionsEmailAsync(user.KeycloakId, new[] { "UPDATE_PASSWORD" });
        }

        // ===================== Forgot password (anonymous, OTP) =====================

        public async Task RequestPasswordResetOtpAsync(string email)
        {
            await EnforceResendCooldownAsync(email, VerificationPurposeEnum.ForgotPassword);

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                // Persist a row (never emailed) so a repeated probe against an unregistered
                // address sees the identical cooldown behavior as a real account — the
                // response never discloses which emails are registered.
                await IssueOtpAsync(email, VerificationPurposeEnum.ForgotPassword, userId: null);
                return;
            }

            var code = await IssueOtpAsync(email, VerificationPurposeEnum.ForgotPassword, user.UserId);
            // Doctors/Staff have no separate username — they log in with this same email — so
            // spelling it out here doubles as the answer to "what's my username?" for anyone
            // who forgot that their email *is* their login.
            await _emailService.SendAsync(email, "Your PrescriptionMS password reset code", BuildOtpEmailBody($"reset the password for your PrescriptionMS account ({email})", code));
        }

        public async Task<bool> VerifyPasswordResetOtpAsync(string email, string code)
        {
            var entity = await ValidateActiveCodeAsync(email, VerificationPurposeEnum.ForgotPassword, code);
            return entity is { UserId: not null };
        }

        public async Task ResetPasswordWithOtpAsync(string email, string code, string newPassword)
        {
            var entity = await ValidateActiveCodeAsync(email, VerificationPurposeEnum.ForgotPassword, code);
            if (entity?.UserId is not long userId)
                throw new BadRequestException("Invalid or expired code.");

            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException("User", userId);

            await _adminClient.SetTemporaryPasswordAsync(user.KeycloakId, newPassword);
            await ConsumeAsync(entity);
        }

        // ===================== Self-service change email / password =====================

        public async Task RequestEmailChangeAsync(string keycloakId, string newEmail)
        {
            var user = await _userRepository.GetByKeycloakIdAsync(keycloakId)
                ?? throw new NotFoundException("User", keycloakId);

            await EnforceResendCooldownAsync(newEmail, VerificationPurposeEnum.ChangeEmail);

            var code = await IssueOtpAsync(newEmail, VerificationPurposeEnum.ChangeEmail, user.UserId, newValue: newEmail);
            await _emailService.SendAsync(newEmail, "Confirm your new PrescriptionMS email address", BuildOtpEmailBody("confirm this email address", code));
        }

        public async Task ConfirmEmailChangeAsync(string keycloakId, string code)
        {
            var user = await _userRepository.GetByKeycloakIdAsync(keycloakId)
                ?? throw new NotFoundException("User", keycloakId);

            var entity = await FindLatestForUserAsync(user.UserId, VerificationPurposeEnum.ChangeEmail)
                ?? throw new BadRequestException("Invalid or expired code.");
            var validated = await ValidateActiveCodeAsync(entity.Email, VerificationPurposeEnum.ChangeEmail, code, expectedUserId: user.UserId);
            if (validated?.NewValue is not string newEmail)
                throw new BadRequestException("Invalid or expired code.");

            await _adminClient.SetUserEmailAsync(user.KeycloakId, newEmail);
            user.Email = newEmail;
            _userRepository.Update(user);

            await ConsumeAsync(validated);
        }

        public async Task RequestPasswordChangeAsync(string keycloakId)
        {
            var user = await _userRepository.GetByKeycloakIdAsync(keycloakId)
                ?? throw new NotFoundException("User", keycloakId);
            if (string.IsNullOrWhiteSpace(user.Email))
                throw new BadRequestException("Add an email address to your profile before changing your password.");

            await EnforceResendCooldownAsync(user.Email, VerificationPurposeEnum.ChangePassword);

            var code = await IssueOtpAsync(user.Email, VerificationPurposeEnum.ChangePassword, user.UserId);
            await _emailService.SendAsync(user.Email, "Your PrescriptionMS password change code", BuildOtpEmailBody($"change the password for your PrescriptionMS account ({user.Email})", code));
        }

        public async Task ConfirmPasswordChangeAsync(string keycloakId, string code, string newPassword)
        {
            var user = await _userRepository.GetByKeycloakIdAsync(keycloakId)
                ?? throw new NotFoundException("User", keycloakId);
            if (string.IsNullOrWhiteSpace(user.Email))
                throw new BadRequestException("Add an email address to your profile before changing your password.");

            var entity = await ValidateActiveCodeAsync(user.Email, VerificationPurposeEnum.ChangePassword, code, expectedUserId: user.UserId)
                ?? throw new BadRequestException("Invalid or expired code.");

            await _adminClient.SetTemporaryPasswordAsync(user.KeycloakId, newPassword);
            await ConsumeAsync(entity);
        }

        // ===================== OTP plumbing =====================

        private async Task EnforceResendCooldownAsync(string email, VerificationPurposeEnum purpose)
        {
            var latest = await _verificationCodeRepository.GetLatestActiveAsync(email, purpose);
            if (latest != null && DateTime.UtcNow < latest.IssuedAt.Add(OtpResendCooldown))
                throw new BadRequestException("Please wait a minute before requesting another code.");
        }

        private async Task<string> IssueOtpAsync(string email, VerificationPurposeEnum purpose, long? userId, string? newValue = null)
        {
            var code = GenerateOtpCode();
            var entity = new VerificationCode
            {
                Purpose = purpose,
                Email = email,
                CodeHash = HashCode(code),
                UserId = userId,
                NewValue = newValue,
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(OtpValidity),
                AttemptCount = 0
            };
            await _verificationCodeRepository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return code;
        }

        private async Task<VerificationCode?> FindLatestForUserAsync(long userId, VerificationPurposeEnum purpose)
        {
            var candidates = await _verificationCodeRepository.FindAsync(
                v => v.UserId == userId && v.Purpose == purpose && v.ConsumedAt == null);
            return candidates.OrderByDescending(v => v.IssuedAt).FirstOrDefault();
        }

        /// <summary>
        /// Validates the most recent active code for email+purpose: not expired, under the
        /// attempt cap, hash matches. Increments AttemptCount on every call (even a
        /// successful one, so a verify-then-confirm pair still counts as 2 of the cap) but
        /// never consumes — callers that finalize an action call <see cref="ConsumeAsync"/>
        /// themselves once the side effect actually happens.
        /// </summary>
        private async Task<VerificationCode?> ValidateActiveCodeAsync(string email, VerificationPurposeEnum purpose, string code, long? expectedUserId = null)
        {
            var entity = await _verificationCodeRepository.GetLatestActiveAsync(email, purpose);
            if (entity == null) return null;
            if (expectedUserId.HasValue && entity.UserId != expectedUserId.Value) return null;
            if (DateTime.UtcNow > entity.ExpiresAt) return null;
            if (entity.AttemptCount >= OtpMaxAttempts) return null;

            entity.AttemptCount++;
            _verificationCodeRepository.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return HashCode(code) == entity.CodeHash ? entity : null;
        }

        private async Task ConsumeAsync(VerificationCode entity)
        {
            entity.ConsumedAt = DateTime.UtcNow;
            _verificationCodeRepository.Update(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        private static string GenerateOtpCode() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        private static string HashCode(string code) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));

        private static string BuildOtpEmailBody(string action, string code) =>
            $"""
            <p>Use this code to {action}:</p>
            <p style="font-size:28px;font-weight:700;letter-spacing:4px;">{code}</p>
            <p>This code expires in 10 minutes. If you didn't request this, you can safely ignore this email.</p>
            """;

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
