using SylviaNG.Prescription.Application.Features.Analytics.Models;
using SylviaNG.Prescription.Application.Features.Prescriptions;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Application.Features.Analytics
{
    public class MedicineAggregationResult
    {
        public Dictionary<string, int> CountsByKey { get; } = new();

        /// <summary>First-seen-casing display label per normalized key.</summary>
        public Dictionary<string, string> LabelByKey { get; } = new();

        /// <summary>Key is (LabelA, LabelB), alphabetically ordered case-insensitive — see <see cref="MedicinePrescribingAggregator.Aggregate"/>.</summary>
        public Dictionary<(string A, string B), int> CoPrescribedPairCounts { get; } = new();
    }

    /// <summary>
    /// The one place every medicine-identity aggregation in Epic M shares — mirrors the
    /// reference prototype's <c>analyticsService.js</c> top-medicines/co-prescription logic,
    /// server-side. Groups by generic name (falling back to brand name when no generic is
    /// recorded) with <see cref="MedicineDuplicateGuard.NormalizeKey"/> called with an empty
    /// strength — a deliberately different call-site than <c>GetMedicineCatalogHandler</c>'s
    /// brand+strength SKU key, since two strengths of the same medicine (e.g. Paracetamol
    /// 500mg/650mg) must count as one medicine here, not two catalog rows.
    /// </summary>
    public static class MedicinePrescribingAggregator
    {
        public static MedicineAggregationResult Aggregate(IEnumerable<PrescriptionRecord> finalizedPrescriptions)
        {
            var result = new MedicineAggregationResult();

            foreach (var prescription in finalizedPrescriptions)
            {
                // De-duplicated within this one prescription only — feeds the co-prescribed
                // pair count, which must not double-count a medicine paired with itself.
                var linesInThisRx = new Dictionary<string, string>();

                foreach (var line in prescription.GetMedicines())
                {
                    var rawName = (line.Generic ?? line.Medicine) ?? string.Empty;
                    var trimmedName = rawName.Trim();
                    if (trimmedName.Length == 0)
                    {
                        continue;
                    }

                    var key = MedicineDuplicateGuard.NormalizeKey(rawName, string.Empty);

                    result.CountsByKey[key] = result.CountsByKey.GetValueOrDefault(key) + 1;
                    if (!result.LabelByKey.ContainsKey(key))
                    {
                        result.LabelByKey[key] = trimmedName;
                    }

                    linesInThisRx.TryAdd(key, result.LabelByKey[key]);
                }

                var keys = linesInThisRx.Keys.ToList();
                for (var i = 0; i < keys.Count; i++)
                {
                    for (var j = i + 1; j < keys.Count; j++)
                    {
                        var labelA = linesInThisRx[keys[i]];
                        var labelB = linesInThisRx[keys[j]];
                        var pairKey = string.CompareOrdinal(labelA.ToLowerInvariant(), labelB.ToLowerInvariant()) <= 0
                            ? (labelA, labelB)
                            : (labelB, labelA);
                        result.CoPrescribedPairCounts[pairKey] = result.CoPrescribedPairCounts.GetValueOrDefault(pairKey) + 1;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Category breakdown: resolves each finalized prescription's medicine lines back to
        /// the catalog's <see cref="Medicine.Category"/> via the same normalized-key match,
        /// falling back to "Uncategorized" for lines that don't match any catalog row (a
        /// discontinued/free-typed medicine, matching the prototype's behavior).
        /// </summary>
        public static List<CategoryCountEntry> BreakdownByCategory(
            IEnumerable<PrescriptionRecord> finalizedPrescriptions,
            List<Medicine> catalog)
        {
            const string uncategorized = "Uncategorized";

            var categoryByKey = catalog
                .GroupBy(m => MedicineDuplicateGuard.NormalizeKey(m.GenericName ?? m.BrandName, string.Empty))
                .ToDictionary(g => g.Key, g => g.First().Category ?? uncategorized);

            var counts = new Dictionary<string, int>();
            foreach (var prescription in finalizedPrescriptions)
            {
                foreach (var line in prescription.GetMedicines())
                {
                    var rawName = (line.Generic ?? line.Medicine) ?? string.Empty;
                    if (rawName.Trim().Length == 0)
                    {
                        continue;
                    }

                    var key = MedicineDuplicateGuard.NormalizeKey(rawName, string.Empty);
                    var category = categoryByKey.GetValueOrDefault(key, uncategorized);
                    counts[category] = counts.GetValueOrDefault(category) + 1;
                }
            }

            return counts
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => new CategoryCountEntry { Category = kvp.Key, Count = kvp.Value })
                .ToList();
        }
    }
}
