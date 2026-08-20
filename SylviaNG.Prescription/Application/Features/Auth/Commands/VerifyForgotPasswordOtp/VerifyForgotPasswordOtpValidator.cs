using FluentValidation;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.VerifyForgotPasswordOtp
{
    public class VerifyForgotPasswordOtpValidator : AbstractValidator<VerifyForgotPasswordOtpCommand>
    {
        public VerifyForgotPasswordOtpValidator()
        {
            RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Request.Code)
                .NotEmpty().WithMessage("Code is required.")
                .Length(6).WithMessage("Code must be 6 digits.");
        }
    }
}
