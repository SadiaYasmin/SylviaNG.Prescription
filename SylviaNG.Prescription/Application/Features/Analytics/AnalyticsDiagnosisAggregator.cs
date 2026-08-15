using SylviaNG.Prescription.Application.Features.Analytics.Models;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Application.Features.Analytics
{
    /// <summary>
    /// Diagnosis aggregation shared by US-075 (Patient Analytics) and US-076 (Executive
    /// Summary) so the case-insensitive dedup/first-seen-casing logic lives in exactly one
    /// place. Ported from the reference prototype's <c>analyticsService.js</c>
    /// <c>topDiagnoses</c>/<c>chronicConditions</c>.
    /// </summary>
    public static class AnalyticsDiagnosisAggregator
    {
        public static List<DiagnosisCountEntry> TopDiagnoses(IEnumerable<PrescriptionRecord> prescriptions, int topN)
        {
            var counts = new Dictionary<string, (string Label, int Count)>();

            foreach (var prescription in prescriptions)
            {
                foreach (var diagnosis in prescription.GetDiagnoses())
                {
                    var text = diagnosis.Text?.Trim() ?? string.Empty;
                    if (text.Length == 0)
                    {
                        continue;
                    }

                    var key = text.ToLowerInvariant();
                    if (counts.TryGetValue(key, out var existing))
                    {
                        counts[key] = (existing.Label, existing.Count + 1);
                    }
                    else
                    {
                        counts[key] = (text, 1);
                    }
                }
            }

            return counts.Values
                .OrderByDescending(v => v.Count)
                .Take(topN)
                .Select(v => new DiagnosisCountEntry { Diagnosis = v.Label, Count = v.Count })
                .ToList();
        }

        /// <summary>
        /// "Chronic" = the same diagnosis text (case-insensitive) appears on more than one
        /// prescription for the same patient — threshold is strictly <c>&gt; 1</c> (2+
        /// occurrences), not 3+, matching the prototype exactly. No time-window constraint.
        /// <see cref="ChronicDiagnosisEntry.PatientName"/> is left blank here — the caller
        /// (a handler with access to <see cref="Patient"/> data) fills it in.
        /// </summary>
        public static List<ChronicDiagnosisEntry> ChronicPatterns(IEnumerable<PrescriptionRecord> prescriptions)
        {
            var perPatient = new Dictionary<long, Dictionary<string, (string Label, int Count)>>();

            foreach (var prescription in prescriptions)
            {
                if (!perPatient.TryGetValue(prescription.PatientId, out var diagnosisCounts))
                {
                    diagnosisCounts = new Dictionary<string, (string Label, int Count)>();
                    perPatient[prescription.PatientId] = diagnosisCounts;
                }

                foreach (var diagnosis in prescription.GetDiagnoses())
                {
                    var text = diagnosis.Text?.Trim() ?? string.Empty;
                    if (text.Length == 0)
                    {
                        continue;
                    }

                    var key = text.ToLowerInvariant();
                    diagnosisCounts[key] = diagnosisCounts.TryGetValue(key, out var existing)
                        ? (existing.Label, existing.Count + 1)
                        : (text, 1);
                }
            }

            var result = new List<ChronicDiagnosisEntry>();
            foreach (var (patientId, diagnosisCounts) in perPatient)
            {
                foreach (var (_, value) in diagnosisCounts)
                {
                    if (value.Count > 1)
                    {
                        result.Add(new ChronicDiagnosisEntry
                        {
                            PatientId = patientId,
                            Diagnosis = value.Label,
                            Occurrences = value.Count
                        });
                    }
                }
            }

            return result.OrderByDescending(e => e.Occurrences).ToList();
        }
    }
}
