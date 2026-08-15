using FluentValidation;

namespace SylviaNG.Prescription.Application.Features.QuickAdd.Commands.UpdateQuickAddPreset
{
    public class UpdateQuickAddPresetValidator : AbstractValidator<UpdateQuickAddPresetCommand>
    {
        public UpdateQuickAddPresetValidator()
        {
            RuleFor(x => x.Request.Label)
                .NotEmpty().WithMessage("Label is required.")
                .MaximumLength(300).WithMessage("Label must not exceed 300 characters.");

            RuleFor(x => x.Request.PayloadJson)
                .NotEmpty().WithMessage("Payload is required.");
        }
    }
}
