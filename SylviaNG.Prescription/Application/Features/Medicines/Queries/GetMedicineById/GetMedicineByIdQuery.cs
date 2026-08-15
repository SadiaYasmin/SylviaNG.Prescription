using MediatR;
using SylviaNG.Prescription.Application.Features.Medicines.Models;

namespace SylviaNG.Prescription.Application.Features.Medicines.Queries.GetMedicineById
{
    /// <summary>Feeds the Admin catalog edit form (US-037).</summary>
    public class GetMedicineByIdQuery : IRequest<MedicineCatalogResponse>
    {
        public long MedicineId { get; set; }

        public GetMedicineByIdQuery(long medicineId)
        {
            MedicineId = medicineId;
        }
    }
}
