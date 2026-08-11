using FluentValidation;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Templates.Commands.CreateTemplate
{
    public class CreateTemplateValidator : AbstractValidator<CreateTemplateCommand>
    {
        public CreateTemplateValidator()
        {
            RuleFor(x => x.Request.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

            RuleFor(x => x.Request.Type)
                .IsInEnum().WithMessage("Type must be a valid template type.");

            RuleFor(x => x.Request.Language)
                .IsInEnum().WithMessage("Language must be a valid template language.");
        }
    }
}
