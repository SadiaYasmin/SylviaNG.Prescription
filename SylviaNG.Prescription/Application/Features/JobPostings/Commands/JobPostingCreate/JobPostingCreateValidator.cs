using FluentValidation;

namespace SylviaNG.Prescription.Application.Features.JobPostings.Commands.JobPostingCreate
{
    public class JobPostingCreateValidator : AbstractValidator<JobPostingCreateCommand>
    {
        public JobPostingCreateValidator()
        {
            RuleFor(x => x.Request.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

            RuleFor(x => x.Request.SiteId)
                .GreaterThan(0).WithMessage("SiteId is required.");

            RuleFor(x => x.Request.NumberOfPositions)
                .GreaterThan(0).WithMessage("Number of positions must be greater than 0.");

            RuleFor(x => x.Request.MinSalary)
                .LessThan(x => x.Request.MaxSalary)
                .When(x => x.Request.MinSalary.HasValue && x.Request.MaxSalary.HasValue)
                .WithMessage("MinSalary must be less than MaxSalary.");

            RuleFor(x => x.Request.ClosingDate)
                .GreaterThan(x => x.Request.PostingDate)
                .When(x => x.Request.PostingDate.HasValue && x.Request.ClosingDate.HasValue)
                .WithMessage("Closing date must be after posting date.");
        }
    }
}
