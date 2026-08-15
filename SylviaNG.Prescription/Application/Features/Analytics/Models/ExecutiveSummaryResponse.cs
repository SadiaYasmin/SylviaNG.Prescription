namespace SylviaNG.Prescription.Application.Features.Analytics.Models
{
    public class MonthOverMonthMetric
    {
        public int Current { get; set; }
        public int Previous { get; set; }

        /// <summary>Null = no baseline (previous was 0, current &gt; 0) — render as "New", not "0%".</summary>
        public double? PercentChange { get; set; }
    }

    /// <summary>US-076: one-glance headline view, Admin only.</summary>
    public class ExecutiveSummaryResponse
    {
        public int TotalPatients { get; set; }
        public int TotalPrescriptions { get; set; }
        public int TotalMedicines { get; set; }
        public int TotalDoctors { get; set; }
        public MonthOverMonthMetric PrescriptionTrend { get; set; } = new();
        public MonthOverMonthMetric NewPatientTrend { get; set; } = new();
        public List<MedicineCountEntry> TopMedicines { get; set; } = new();
        public List<DiagnosisCountEntry> TopDiagnoses { get; set; } = new();
        public List<DoctorCountEntry> TopActiveDoctors { get; set; } = new();
    }
}
