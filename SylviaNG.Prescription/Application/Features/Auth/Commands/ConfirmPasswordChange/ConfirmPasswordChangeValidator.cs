using FluentValidation;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.ConfirmPasswordChange
{
    public class ConfirmPasswordChangeValidator : AbstractValidator<ConfirmPasswordChangeCommand>
    {
        public ConfirmPasswordChangeValidator()
        {
            RuleFor(x => x.Request.Code)
                .NotEmpty().WithMessage("Code is required.")
                .Length(6).WithMessage("Code must be 6 digits.");
            RuleFor(x => x.Request.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.");
        }
    }
}
