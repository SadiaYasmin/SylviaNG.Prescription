namespace SylviaNG.Prescription.Domain.Enums
{
    /// <summary>
    /// What a <see cref="Entities.VerificationCode"/> row is being used to confirm —
    /// drives which side-effect happens once the code is verified.
    /// </summary>
    public enum VerificationPurposeEnum
    {
        ForgotPassword,
        ChangeEmail,
        ChangePassword
    }
}
