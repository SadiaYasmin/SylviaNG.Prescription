using FluentValidation;

namespace SylviaNG.Prescription.Application.Features.Departments.Commands.CreateDepartment
{
    public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentCommand>
    {
        public CreateDepartmentValidator()
        {
            RuleFor(x => x.Request.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
        }
    }
}
