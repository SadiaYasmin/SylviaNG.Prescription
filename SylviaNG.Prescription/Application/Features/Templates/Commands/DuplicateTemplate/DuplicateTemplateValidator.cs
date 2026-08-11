using FluentValidation;

namespace SylviaNG.Prescription.Application.Features.Templates.Commands.DuplicateTemplate
{
    public class DuplicateTemplateValidator : AbstractValidator<DuplicateTemplateCommand>
    {
        public DuplicateTemplateValidator()
        {
            RuleFor(x => x.TemplateId)
                .GreaterThan(0).WithMessage("TemplateId must be a positive number.");
        }
    }
}
