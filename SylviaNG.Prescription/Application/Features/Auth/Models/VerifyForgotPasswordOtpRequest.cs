namespace SylviaNG.Prescription.Application.Features.Auth.Models
{
    public class VerifyForgotPasswordOtpRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class VerifyForgotPasswordOtpResponse
    {
        public bool Valid { get; set; }
    }
}
