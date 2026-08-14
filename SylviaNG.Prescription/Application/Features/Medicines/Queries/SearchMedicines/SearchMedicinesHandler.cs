using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Features.Medicines.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;

namespace SylviaNG.Prescription.Application.Features.Medicines.Queries.SearchMedicines
{
    public class SearchMedicinesHandler : IRequestHandler<SearchMedicinesQuery, List<MedicineSummaryResponse>>
    {
        private readonly IMedicineRepository _medicineRepository;

        public SearchMedicinesHandler(IMedicineRepository medicineRepository)
        {
            _medicineRepository = medicineRepository;
        }

        public async Task<List<MedicineSummaryResponse>> Handle(SearchMedicinesQuery query, CancellationToken cancellationToken)
        {
            var medicines = _medicineRepository.Query().Where(m => m.Active);

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var term = query.SearchTerm.Trim().ToLower();
                medicines = medicines.Where(m =>
                    m.BrandName.ToLower().Contains(term) ||
                    (m.GenericName != null && m.GenericName.ToLower().Contains(term)));
            }

            var results = await medicines.OrderBy(m => m.BrandName).Take(50).ToListAsync(cancellationToken);
            return results.Select(m => m.ToSummaryResponse()).ToList();
        }
    }
}
