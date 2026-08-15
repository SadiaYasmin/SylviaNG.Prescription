using MediatR;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Medicines.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;

namespace SylviaNG.Prescription.Application.Features.Medicines.Queries.GetMedicineById
{
    public class GetMedicineByIdHandler : IRequestHandler<GetMedicineByIdQuery, MedicineCatalogResponse>
    {
        private readonly IMedicineRepository _medicineRepository;

        public GetMedicineByIdHandler(IMedicineRepository medicineRepository)
        {
            _medicineRepository = medicineRepository;
        }

        public async Task<MedicineCatalogResponse> Handle(GetMedicineByIdQuery query, CancellationToken cancellationToken)
        {
            var medicine = await _medicineRepository.GetByIdAsync(query.MedicineId)
                ?? throw new NotFoundException("Medicine", query.MedicineId);

            // Total Prescribed isn't needed on the edit form itself — 0 is a cheap stand-in
            // rather than paying for the same in-memory aggregation GetMedicineCatalogHandler
            // does just to populate a field this screen never displays.
            return medicine.ToCatalogResponse(0);
        }
    }
}
