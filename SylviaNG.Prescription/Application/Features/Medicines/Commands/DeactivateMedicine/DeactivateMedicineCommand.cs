using MediatR;

namespace SylviaNG.Prescription.Application.Features.Medicines.Commands.DeactivateMedicine
{
    public class DeactivateMedicineCommand : IRequest<Unit>
    {
        public long MedicineId { get; set; }

        public DeactivateMedicineCommand(long medicineId)
        {
            MedicineId = medicineId;
        }
    }
}
