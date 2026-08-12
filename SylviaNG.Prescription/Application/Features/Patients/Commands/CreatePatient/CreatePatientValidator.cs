using FluentValidation;
using SylviaNG.Prescription.Application.Common.Validators;

namespace SylviaNG.Prescription.Application.Features.Patients.Commands.CreatePatient
{
    public class CreatePatientValidator : AbstractValidator<CreatePatientCommand>
    {
        public CreatePatientValidator()
        {
            RuleFor(x => x.Request.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

            RuleFor(x => x.Request.Phone)
                .NotEmpty().WithMessage("Phone is required.")
                .Matches(PhoneValidation.BangladeshMobileRegex)
                .WithMessage(PhoneValidation.ValidationMessage);

            // DOB is the preferred source of truth; Age is only a fallback when DOB isn't
            // known, so it becomes mandatory exactly when DOB is absent. The reverse isn't
            // required — DOB alone is fine without Age.
            RuleFor(x => x.Request.Age)
                .NotNull().WithMessage("Age is required when date of birth is not provided.")
                .When(x => x.Request.DateOfBirth is null);

            // Allergy is optional as a whole (both AllergyPresetId and AllergyOtherText may
            // be omitted/null). But once the caller signals "Other" by sending a non-null
            // AllergyOtherText without a preset id, that text must not be blank.
            RuleFor(x => x.Request.AllergyOtherText)
                .NotEmpty().WithMessage("Please describe the allergy when no preset allergy is selected.")
                .When(x => x.Request.AllergyPresetId is null && x.Request.AllergyOtherText is not null);
        }
    }
}
