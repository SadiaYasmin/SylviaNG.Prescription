using FluentValidation;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.ResetPasswordWithOtp
{
    public class ResetPasswordWithOtpValidator : AbstractValidator<ResetPasswordWithOtpCommand>
    {
        public ResetPasswordWithOtpValidator()
        {
            RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Request.Code)
                .NotEmpty().WithMessage("Code is required.")
                .Length(6).WithMessage("Code must be 6 digits.");
            RuleFor(x => x.Request.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.");
        }
    }
}
