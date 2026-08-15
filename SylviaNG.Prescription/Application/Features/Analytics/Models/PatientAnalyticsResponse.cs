namespace SylviaNG.Prescription.Application.Features.Analytics.Models
{
    /// <summary>US-075: patient population analytics, Admin only.</summary>
    public class PatientAnalyticsResponse
    {
        public int NewPatients { get; set; }
        public int ReturningPatients { get; set; }
        public List<TrendPoint> NewRegistrationTrend { get; set; } = new();
        public double AverageVisitsPerPatient { get; set; }
        public List<DiagnosisCountEntry> TopDiagnoses { get; set; } = new();
        public List<ChronicDiagnosisEntry> ChronicDiagnosisPatterns { get; set; } = new();
    }
}
