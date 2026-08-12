using FluentValidation;
using SylviaNG.Prescription.SharedKernel.Utils;

namespace SylviaNG.Prescription.Application.Features.Consultations.Commands.CreateConsultation
{
    public class CreateConsultationValidator : AbstractValidator<CreateConsultationCommand>
    {
        public CreateConsultationValidator()
        {
            RuleFor(x => x.Request.PatientId)
                .GreaterThan(0).WithMessage("PatientId is required.");

            RuleFor(x => x.Request.DoctorId)
                .GreaterThan(0).WithMessage("DoctorId is required.");

            // Consultation check-ins are created for today's queue only. A null value is
            // allowed so the handler can apply the server-side local date.
            RuleFor(x => x.Request.VisitDate)
                .Must(date => date!.Value == DateTimeUtility.TodayLocal())
                .WithMessage("Consultations can only be created for today.")
                .When(x => x.Request.VisitDate.HasValue);
        }
    }
}
