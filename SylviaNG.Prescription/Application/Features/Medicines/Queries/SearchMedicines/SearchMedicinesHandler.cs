using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Features.Medicines.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;

namespace SylviaNG.Prescription.Application.Features.Medicines.Queries.SearchMedicines
{
    public class SearchMedicinesHandler : IRequestHandler<SearchMedicinesQuery, MedicineSearchListResponse>
    {
        private readonly IMedicineRepository _medicineRepository;

        public SearchMedicinesHandler(IMedicineRepository medicineRepository)
        {
            _medicineRepository = medicineRepository;
        }

        public async Task<MedicineSearchListResponse> Handle(SearchMedicinesQuery query, CancellationToken cancellationToken)
        {
            var medicines = _medicineRepository.Query().Where(m => m.Active);

            if (!query.IncludeUnregistered)
            {
                medicines = medicines.Where(m => m.DgdaRegistered);
            }

            var term = query.SearchTerm?.Trim().ToLower();

            // Blank term = deliberate "browse everything" (Staff's plain catalog list uses this
            // endpoint with no query on page load) — only a *typed-but-too-short* term (1 char)
            // is rejected, matching the 2-char autocomplete minimum without breaking that browse.
            if (term is { Length: 1 })
            {
                return new MedicineSearchListResponse { PageNumber = query.Page, PageSize = query.PageSize };
            }

            if (!string.IsNullOrEmpty(term))
            {
                medicines = medicines.Where(m =>
                    m.BrandName.ToLower().Contains(term) ||
                    (m.GenericName != null && m.GenericName.ToLower().Contains(term)));
            }

            // Real total (not just "however many fit in the capped page") — Admin, Doctor, and
            // Staff all browse/search the exact same underlying catalog and count; previously
            // this endpoint silently capped at 50 rows with no real total, so Staff's browse
            // header understated the catalog size by ~400x and had no way to page further.
            var totalCount = await medicines.CountAsync(cancellationToken);

            var ordered = string.IsNullOrEmpty(term)
                ? medicines.OrderBy(m => m.BrandName)
                : medicines.OrderBy(m => m.BrandName.ToLower().StartsWith(term) ? 0 : 1)
                    .ThenBy(m => (m.GenericName != null && m.GenericName.ToLower().StartsWith(term)) ? 0 : 1)
                    .ThenBy(m => m.BrandName);

            var results = await ordered
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return new MedicineSearchListResponse
            {
                Medicines = results.Select(m => m.ToSummaryResponse()).ToList(),
                TotalCount = totalCount,
                PageNumber = query.Page,
                PageSize = query.PageSize,
            };
        }
    }
}
