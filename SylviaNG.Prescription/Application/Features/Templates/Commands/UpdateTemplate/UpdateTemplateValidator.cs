using FluentValidation;

namespace SylviaNG.Prescription.Application.Features.Templates.Commands.UpdateTemplate
{
    public class UpdateTemplateValidator : AbstractValidator<UpdateTemplateCommand>
    {
        private const string HexColorRegex = "^#[0-9A-Fa-f]{6}$";

        public UpdateTemplateValidator()
        {
            RuleFor(x => x.Request.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

            RuleFor(x => x.Request.Config.Header.Height)
                .InclusiveBetween(30, 300).WithMessage("Header height must be between 30 and 300.");

            RuleFor(x => x.Request.Config.Footer.Height)
                .InclusiveBetween(30, 300).WithMessage("Footer height must be between 30 and 300.");

            RuleFor(x => x.Request.Config.Style.FontSize)
                .InclusiveBetween(8, 32).WithMessage("Font size must be between 8 and 32.");

            RuleFor(x => x.Request.Config.Style.SectionSpacing)
                .GreaterThanOrEqualTo(0).WithMessage("Section spacing must not be negative.");

            RuleFor(x => x.Request.Config.Style.BorderRadius)
                .GreaterThanOrEqualTo(0).WithMessage("Border radius must not be negative.");

            RuleFor(x => x.Request.Config.Header.BgColor)
                .Matches(HexColorRegex).WithMessage("Header background color must be a valid hex color (e.g. #0F766E).")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Config.Header.BgColor));

            RuleFor(x => x.Request.Config.Footer.BgColor)
                .Matches(HexColorRegex).WithMessage("Footer background color must be a valid hex color (e.g. #0F766E).")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Config.Footer.BgColor));

            RuleFor(x => x.Request.Config.Style.AccentColor)
                .Matches(HexColorRegex).WithMessage("Accent color must be a valid hex color (e.g. #0F766E).")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Config.Style.AccentColor));
        }
    }
}
