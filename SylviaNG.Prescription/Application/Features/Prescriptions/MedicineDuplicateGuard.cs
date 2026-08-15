using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;

namespace SylviaNG.Prescription.Application.Features.Prescriptions
{
    /// <summary>
    /// US-022's duplicate-medicine guard, enforced server-side (the frontend also blocks
    /// this as a UX nicety, but this is the real gate — same defense-in-depth reasoning as
    /// every RBAC rule in this codebase). Two lines collide when their medicine name +
    /// strength match after trim/case-normalization; dosage/frequency/duration/instructions
    /// are ignored, matching the reference prototype's exact matching key.
    /// </summary>
    public static class MedicineDuplicateGuard
    {
        public static void EnsureNoDuplicates(List<MedicineItem> medicines)
        {
            var seen = new HashSet<string>();
            foreach (var medicine in medicines)
            {
                var key = NormalizeKey(medicine.Medicine, medicine.Strength);
                if (!seen.Add(key))
                {
                    throw new BadRequestException(
                        $"Duplicate medicine: \"{medicine.Medicine}\" ({medicine.Strength}) already appears in this prescription. Edit the existing line instead of adding it again.");
                }
            }
        }

        /// <summary>
        /// The trim/lowercase name+strength key used both here and by Epic F's catalog CRUD
        /// (duplicate brand+strength check) and prescribing-analytics aggregation — one
        /// normalization function shared by every feature that needs to match a free-typed
        /// prescription line back to a catalog entry.
        /// </summary>
        public static string NormalizeKey(string? name, string? strength) => Normalize(name) + "|" + Normalize(strength);

        private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();
    }
}
