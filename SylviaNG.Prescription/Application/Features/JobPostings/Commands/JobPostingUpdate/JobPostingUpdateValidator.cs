using FluentValidation;

namespace SylviaNG.Prescription.Application.Features.JobPostings.Commands.JobPostingUpdate
{
    public class JobPostingUpdateValidator : AbstractValidator<JobPostingUpdateCommand>
    {
        public JobPostingUpdateValidator()
        {
            RuleFor(x => x.JobPostingId)
                .GreaterThan(0).WithMessage("JobPostingId is required.");

            RuleFor(x => x.Request.Title)
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters.")
                .When(x => x.Request.Title != null);

            RuleFor(x => x.Request.MinSalary)
                .LessThan(x => x.Request.MaxSalary)
                .When(x => x.Request.MinSalary.HasValue && x.Request.MaxSalary.HasValue)
                .WithMessage("MinSalary must be less than MaxSalary.");
        }
    }
}
