using FluentValidation;
using SylviaNG.Prescription.Application.Common.Validators;

namespace SylviaNG.Prescription.Application.Features.Staffs.Commands.UpdateStaff
{
    public class UpdateStaffValidator : AbstractValidator<UpdateStaffCommand>
    {
        public UpdateStaffValidator()
        {
            RuleFor(x => x.StaffId)
                .GreaterThan(0).WithMessage("A valid staff id is required.");

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
