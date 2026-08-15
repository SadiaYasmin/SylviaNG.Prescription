using FluentValidation;

namespace SylviaNG.Prescription.Application.Features.Medicines.Commands.UpdateMedicine
{
    public class UpdateMedicineValidator : AbstractValidator<UpdateMedicineCommand>
    {
        public UpdateMedicineValidator()
        {
            RuleFor(x => x.Request.BrandName)
                .NotEmpty().WithMessage("Brand name is required.")
                .MaximumLength(200).WithMessage("Brand name must not exceed 200 characters.");
        }
    }
}
