using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Features.Medicines.Models;
using SylviaNG.Prescription.Application.Features.Prescriptions;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Medicines.Queries.GetMedicineCatalog
{
    public class GetMedicineCatalogHandler : IRequestHandler<GetMedicineCatalogQuery, MedicineCatalogListResponse>
    {
        private readonly IMedicineRepository _medicineRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;

        public GetMedicineCatalogHandler(
            IMedicineRepository medicineRepository,
            IPrescriptionRepository prescriptionRepository,
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository)
        {
            _medicineRepository = medicineRepository;
            _prescriptionRepository = prescriptionRepository;
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
        }

        public async Task<MedicineCatalogListResponse> Handle(GetMedicineCatalogQuery query, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                query.KeycloakId, _userRepository, _staffRepository, _doctorRepository);

            var medicines = _medicineRepository.Query();
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var term = query.SearchTerm.Trim().ToLower();
                medicines = medicines.Where(m =>
                    m.BrandName.ToLower().Contains(term) ||
                    (m.GenericName != null && m.GenericName.ToLower().Contains(term)));
            }
            var catalog = await medicines.ToListAsync(cancellationToken);

            // No FK from a prescription's line items back to Medicine.MedicineId (they're a
            // free-typed text snapshot — see PrescriptionRecord's doc comment), so "Total
            // Prescribed" can't be a SQL join/GroupBy. Aggregate in memory instead, keyed by
            // the same normalized name+strength MedicineDuplicateGuard already uses elsewhere.
            // Revisit as a real SQL aggregate if a MedicineId FK is ever added to line items.
            var finalizedQuery = _prescriptionRepository.Query()
                .Where(p => p.Status == PrescriptionStatusEnum.Finalized);
            if (caller.Role == UserRoleEnum.Doctor)
            {
                finalizedQuery = finalizedQuery.Where(p => p.DoctorId == caller.DoctorId!.Value);
            }
            var finalized = await finalizedQuery.ToListAsync(cancellationToken);

            var counts = new Dictionary<string, int>();
            foreach (var prescription in finalized)
            {
                foreach (var line in prescription.GetMedicines())
                {
                    var key = MedicineDuplicateGuard.NormalizeKey(line.Medicine, line.Strength);
                    counts[key] = counts.GetValueOrDefault(key) + 1;
                }
            }

            var ranked = catalog
                .Select(m =>
                {
                    var key = MedicineDuplicateGuard.NormalizeKey(m.BrandName, m.Strength);
                    return m.ToCatalogResponse(counts.GetValueOrDefault(key));
                })
                .OrderByDescending(m => m.TotalPrescribed)
                .ToList();

            return new MedicineCatalogListResponse
            {
                Medicines = ranked.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList(),
                TotalCount = ranked.Count,
                PageNumber = query.Page,
                PageSize = query.PageSize,
            };
        }
    }
}
