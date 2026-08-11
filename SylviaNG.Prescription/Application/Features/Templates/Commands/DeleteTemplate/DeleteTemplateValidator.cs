using FluentValidation;

namespace SylviaNG.Prescription.Application.Features.Templates.Commands.DeleteTemplate
{
    public class DeleteTemplateValidator : AbstractValidator<DeleteTemplateCommand>
    {
        public DeleteTemplateValidator()
        {
            RuleFor(x => x.TemplateId)
                .GreaterThan(0).WithMessage("TemplateId must be a positive number.");
        }
    }
}
