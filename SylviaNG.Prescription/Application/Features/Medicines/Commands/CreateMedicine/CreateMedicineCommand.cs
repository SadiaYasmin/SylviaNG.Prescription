using MediatR;
using SylviaNG.Prescription.Application.Features.Medicines.Models;

namespace SylviaNG.Prescription.Application.Features.Medicines.Commands.CreateMedicine
{
    public class CreateMedicineCommand : IRequest<MedicineCatalogResponse>
    {
        public CreateMedicineRequest Request { get; set; }

        public CreateMedicineCommand(CreateMedicineRequest request)
        {
            Request = request;
        }
    }
}
