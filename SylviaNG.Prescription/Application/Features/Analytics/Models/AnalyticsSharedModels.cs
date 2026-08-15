namespace SylviaNG.Prescription.Application.Features.Analytics.Models
{
    public class TrendPoint
    {
        public string BucketKey { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class MedicineCountEntry
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class CategoryCountEntry
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class DiagnosisCountEntry
    {
        public string Diagnosis { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class CoPrescribedPairEntry
    {
        public string MedicineA { get; set; } = string.Empty;
        public string MedicineB { get; set; } = string.Empty;
        public string PairLabel { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ChronicDiagnosisEntry
    {
        public long PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public int Occurrences { get; set; }
    }

    public class DoctorCountEntry
    {
        public long DoctorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
