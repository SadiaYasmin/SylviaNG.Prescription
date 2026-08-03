using FluentValidation;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.CreateUserAccount
{
    public class CreateUserAccountValidator : AbstractValidator<CreateUserAccountCommand>
    {
        public CreateUserAccountValidator()
        {
            RuleFor(x => x.Request.Username)
                .NotEmpty().WithMessage("Username is required.")
                .MaximumLength(100).WithMessage("Username must not exceed 100 characters.");

            RuleFor(x => x.Request.Email)
                .EmailAddress().WithMessage("Email must be a valid email address.")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Email));

            RuleFor(x => x.Request.Role)
                .IsInEnum().WithMessage("Role must be Admin, Doctor, or Staff.");
        }
    }
}
