namespace SylviaNG.Prescription.Application.Features.Analytics.Models
{
    /// <summary>US-074: finalized-prescription volume trend, Day/Week/Month toggle.</summary>
    public class PrescriptionVolumeTrendResponse
    {
        public AnalyticsGranularity Granularity { get; set; }
        public List<TrendPoint> Points { get; set; } = new();
    }
}
