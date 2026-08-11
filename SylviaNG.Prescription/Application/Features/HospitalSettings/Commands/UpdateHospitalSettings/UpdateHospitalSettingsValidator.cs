using FluentValidation;

namespace SylviaNG.Prescription.Application.Features.HospitalSettings.Commands.UpdateHospitalSettings
{
    public class UpdateHospitalSettingsValidator : AbstractValidator<UpdateHospitalSettingsCommand>
    {
        public UpdateHospitalSettingsValidator()
        {
            RuleFor(x => x.Request.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

            RuleFor(x => x.Request.Phone)
                .NotEmpty().WithMessage("Phone is required.");

            RuleFor(x => x.Request.Email)
                .EmailAddress().WithMessage("Email must be a valid email address.")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Email));
        }
    }
}
