namespace SylviaNG.Prescription.Application.Features.Auth.Models
{
    public class ConfirmEmailChangeRequest
    {
        public string Code { get; set; } = string.Empty;
    }

    public class ConfirmPasswordChangeRequest
    {
        public string Code { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
