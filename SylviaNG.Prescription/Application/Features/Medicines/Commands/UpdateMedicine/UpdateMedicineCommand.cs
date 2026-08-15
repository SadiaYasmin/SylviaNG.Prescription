using MediatR;
using SylviaNG.Prescription.Application.Features.Medicines.Models;

namespace SylviaNG.Prescription.Application.Features.Medicines.Commands.UpdateMedicine
{
    public class UpdateMedicineCommand : IRequest<MedicineCatalogResponse>
    {
        public long MedicineId { get; set; }
        public UpdateMedicineRequest Request { get; set; }

        public UpdateMedicineCommand(long medicineId, UpdateMedicineRequest request)
        {
            MedicineId = medicineId;
            Request = request;
        }
    }
}
