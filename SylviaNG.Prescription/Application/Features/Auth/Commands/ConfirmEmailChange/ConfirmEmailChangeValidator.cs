using FluentValidation;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.ConfirmEmailChange
{
    public class ConfirmEmailChangeValidator : AbstractValidator<ConfirmEmailChangeCommand>
    {
        public ConfirmEmailChangeValidator()
        {
            RuleFor(x => x.Request.Code)
                .NotEmpty().WithMessage("Code is required.")
                .Length(6).WithMessage("Code must be 6 digits.");
        }
    }
}
