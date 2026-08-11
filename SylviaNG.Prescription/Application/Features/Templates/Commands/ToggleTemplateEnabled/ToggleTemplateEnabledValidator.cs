using FluentValidation;

namespace SylviaNG.Prescription.Application.Features.Templates.Commands.ToggleTemplateEnabled
{
    public class ToggleTemplateEnabledValidator : AbstractValidator<ToggleTemplateEnabledCommand>
    {
        public ToggleTemplateEnabledValidator()
        {
            RuleFor(x => x.TemplateId)
                .GreaterThan(0).WithMessage("TemplateId must be a positive number.");
        }
    }
}
