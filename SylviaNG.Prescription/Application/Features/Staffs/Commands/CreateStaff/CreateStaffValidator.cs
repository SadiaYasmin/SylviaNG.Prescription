using FluentValidation;
using SylviaNG.Prescription.Application.Common.Validators;

namespace SylviaNG.Prescription.Application.Features.Staffs.Commands.CreateStaff
{
    public class CreateStaffValidator : AbstractValidator<CreateStaffCommand>
    {
        public CreateStaffValidator()
        {
            RuleFor(x => x.Request.Username)
                .NotEmpty().WithMessage("Username is required.")
                .MaximumLength(100).WithMessage("Username must not exceed 100 characters.");

            RuleFor(x => x.Request.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(200).WithMessage("Full name must not exceed 200 characters.");

            RuleFor(x => x.Request.Email)
                .EmailAddress().WithMessage("Email must be a valid email address.")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Email));

            RuleFor(x => x.Request.Phone)
                .NotEmpty().WithMessage("Phone is required.")
                .Matches(PhoneValidation.BangladeshMobileRegex)
                .WithMessage(PhoneValidation.ValidationMessage);
        }
    }
}
