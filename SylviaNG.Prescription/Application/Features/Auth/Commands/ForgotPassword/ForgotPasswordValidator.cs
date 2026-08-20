using FluentValidation;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordValidator()
        {
            RuleFor(x => x.Request.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");
        }
    }
}
