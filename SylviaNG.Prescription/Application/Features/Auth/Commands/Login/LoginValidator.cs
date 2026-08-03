using FluentValidation;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.Login
{
    public class LoginValidator : AbstractValidator<LoginCommand>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Request.Username)
                .NotEmpty().WithMessage("Username is required.");

            RuleFor(x => x.Request.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}
