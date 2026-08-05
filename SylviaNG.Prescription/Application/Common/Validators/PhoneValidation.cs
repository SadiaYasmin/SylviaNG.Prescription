namespace SylviaNG.Prescription.Application.Common.Validators
{
    /// <summary>
    /// Bangladesh mobile numbers: exactly 11 digits, starting with the operator
    /// prefix 013-019. Mirrors the frontend's client-side check (reference prototype's
    /// phoneValidation.js) — enforced here too since the frontend must never be the
    /// sole enforcement of a validation rule.
    /// </summary>
    public static class PhoneValidation
    {
        public const string BangladeshMobileRegex = "^01[3-9]\\d{8}$";
        public const string ValidationMessage = "Enter a valid Bangladesh mobile number (11 digits, starting with 013-019).";
    }
}
