using FluentValidation;
using SylviaNG.Prescription.Application.Common.Validators;

namespace SylviaNG.Prescription.Application.Features.Patients.Commands.UpdatePatient
{
    public class UpdatePatientValidator : AbstractValidator<UpdatePatientCommand>
    {
        public UpdatePatientValidator()
        {
            RuleFor(x => x.PatientId)
                .GreaterThan(0).WithMessage("A valid patient id is required.");

            RuleFor(x => x.Request.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

            RuleFor(x => x.Request.Phone)
                .NotEmpty().WithMessage("Phone is required.")
                .Matches(PhoneValidation.BangladeshMobileRegex)
                .WithMessage(PhoneValidation.ValidationMessage);

            RuleFor(x => x.Request.Age)
                .NotNull().WithMessage("Age is required when date of birth is not provided.")
                .When(x => x.Request.DateOfBirth is null);

            RuleFor(x => x.Request.AllergyOtherText)
                .NotEmpty().WithMessage("Please describe the allergy when no preset allergy is selected.")
                .When(x => x.Request.AllergyPresetId is null && x.Request.AllergyOtherText is not null);
        }
    }
}
