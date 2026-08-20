using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Audit;

namespace SylviaNG.Prescription.Domain.Entities
{
    /// <summary>
    /// A single-use, time-limited OTP issued for forgot-password, change-email, or
    /// change-password confirmation. Only <see cref="CodeHash"/> (SHA-256) is ever stored —
    /// the plaintext code exists only in memory long enough to email it. One row per
    /// requested code; a fresh request always issues a new row rather than reusing one, so
    /// old codes for the same email+purpose are superseded (see OtpService).
    /// </summary>
    public class VerificationCode : Audit
    {
        public long VerificationCodeId { get; set; }
        public VerificationPurposeEnum Purpose { get; set; }

        /// <summary>The email the code was sent to (target address to verify).</summary>
        public string Email { get; set; } = string.Empty;

        public string CodeHash { get; set; } = string.Empty;

        /// <summary>
        /// The account this code acts on. Null for ForgotPassword until the code is
        /// generated (resolved internally from <see cref="Email"/> but never disclosed to
        /// the client either way — see OtpService's generic-response handling).
        /// </summary>
        public long? UserId { get; set; }

        /// <summary>Only set for ChangeEmail — the new address pending confirmation.</summary>
        public string? NewValue { get; set; }

        /// <summary>
        /// Set explicitly at creation — <c>Audit.CreatedAt</c> is never auto-populated in
        /// this codebase, so it can't be relied on for "most recent code" ordering.
        /// </summary>
        public DateTime IssuedAt { get; set; }

        public DateTime ExpiresAt { get; set; }
        public DateTime? ConsumedAt { get; set; }
        public int AttemptCount { get; set; }
    }
}
