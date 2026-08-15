namespace SylviaNG.Prescription.Application.Features.Analytics.Models
{
    /// <summary>US-072: hospital-wide medicine/prescription analytics, Admin only.</summary>
    public class MedicineAnalyticsResponse
    {
        public List<MedicineCountEntry> TopPrescribedMedicines { get; set; } = new();
        public List<CategoryCountEntry> CategoryBreakdown { get; set; } = new();
        public List<MedicineCountEntry> RarelyUsedMedicines { get; set; } = new();
        public List<CoPrescribedPairEntry> CoPrescribedPairs { get; set; } = new();
    }
}
