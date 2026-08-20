using FluentValidation;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.RequestEmailChange
{
    public class RequestEmailChangeValidator : AbstractValidator<RequestEmailChangeCommand>
    {
        public RequestEmailChangeValidator()
        {
            RuleFor(x => x.Request.NewEmail)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");
        }
    }
}
