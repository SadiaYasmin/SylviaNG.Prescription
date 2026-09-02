using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Features.Analytics.Models;
using SylviaNG.Prescription.Application.Features.Prescriptions;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetMedicineAnalytics
{
    /// <summary>
    /// US-072. Hospital-wide, Finalized-only (matching this codebase's own "prescribed"
    /// convention from <c>GetMedicineCatalogHandler</c>) — one pass over the finalized
    /// prescriptions via <see cref="MedicinePrescribingAggregator"/> feeds all four widgets.
    /// </summary>
    public class GetMedicineAnalyticsHandler : IRequestHandler<GetMedicineAnalyticsQuery, MedicineAnalyticsResponse>
    {
        private readonly IMedicineRepository _medicineRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;

        public GetMedicineAnalyticsHandler(IMedicineRepository medicineRepository, IPrescriptionRepository prescriptionRepository)
        {
            _medicineRepository = medicineRepository;
            _prescriptionRepository = prescriptionRepository;
        }

        public async Task<MedicineAnalyticsResponse> Handle(GetMedicineAnalyticsQuery query, CancellationToken cancellationToken)
        {
            var from = DateTime.SpecifyKind(query.From, DateTimeKind.Utc);
            var to = DateTime.SpecifyKind(query.To, DateTimeKind.Utc);

            var catalog = await _medicineRepository.Query().ToListAsync(cancellationToken);
            var finalized = await _prescriptionRepository.Query()
                .Where(p => p.Status == PrescriptionStatusEnum.Finalized && p.FinalizedAt >= from && p.FinalizedAt < to)
                .ToListAsync(cancellationToken);

            var aggregation = MedicinePrescribingAggregator.Aggregate(finalized);

            var totalPrescriptions = finalized.Count;
            var totalMedicinesPrescribed = aggregation.CountsByKey.Values.Sum();
            var uniqueMedicinesPrescribed = aggregation.CountsByKey.Count;
            var avgMedicinesPerPrescription = AnalyticsMath.SafeDivide(totalMedicinesPrescribed, totalPrescriptions, 2);

            var topPrescribed = aggregation.CountsByKey
                .OrderByDescending(kvp => kvp.Value)
                .Take(query.TopN)
                .Select(kvp => new MedicineCountEntry { Name = aggregation.LabelByKey[kvp.Key], Count = kvp.Value })
                .ToList();

            var categoryBreakdown = MedicinePrescribingAggregator.BreakdownByCategory(finalized, catalog);

            // Iterates every catalog row (not deduplicated by generic) so two brands sharing
            // a rarely-prescribed generic both surface — matches the prototype's row-based
            // "medicines master list" iteration.
            var rarelyUsed = catalog
                .Select(m =>
                {
                    var displayName = m.GenericName ?? m.BrandName;
                    var key = MedicineDuplicateGuard.NormalizeKey(displayName, string.Empty);
                    return new MedicineCountEntry { Name = displayName, Count = aggregation.CountsByKey.GetValueOrDefault(key) };
                })
                .Where(e => e.Count <= query.RareThreshold)
                .OrderBy(e => e.Count)
                .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var coPrescribedPairs = aggregation.CoPrescribedPairCounts
                .OrderByDescending(kvp => kvp.Value)
                .Take(query.PairTopN)
                .Select(kvp => new CoPrescribedPairEntry
                {
                    MedicineA = kvp.Key.A,
                    MedicineB = kvp.Key.B,
                    PairLabel = $"{kvp.Key.A} + {kvp.Key.B}",
                    Count = kvp.Value
                })
                .ToList();

            return new MedicineAnalyticsResponse
            {
                TotalPrescriptions = totalPrescriptions,
                TotalMedicinesPrescribed = totalMedicinesPrescribed,
                UniqueMedicinesPrescribed = uniqueMedicinesPrescribed,
                AvgMedicinesPerPrescription = avgMedicinesPerPrescription,
                TopPrescribedMedicines = topPrescribed,
                CategoryBreakdown = categoryBreakdown,
                RarelyUsedMedicines = rarelyUsed,
                CoPrescribedPairs = coPrescribedPairs
            };
        }
    }
}
