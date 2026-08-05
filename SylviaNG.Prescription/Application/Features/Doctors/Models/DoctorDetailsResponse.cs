namespace SylviaNG.Prescription.Application.Features.Doctors.Models
{
    /// <summary>
    /// Per-doctor performance drill-down (US-055). Every field here is a real,
    /// stably-shaped aggregate — currently zero/empty because Consultation/Prescription
    /// entities don't exist yet (Epic C/D). Once those land, these queries get real
    /// joins; this response contract does not need to change.
    /// </summary>
    public class DoctorPerformanceStats
    {
        public int TotalPatientsConsulted { get; set; }
        public int TotalPrescriptions { get; set; }
        public int TodayPrescriptions { get; set; }
        public int ThisMonthPrescriptions { get; set; }
        public double AvgPrescriptionsPerConsultation { get; set; }
        public double AvgMedicinesPerPrescription { get; set; }
        public int TotalMedicinesPrescribed { get; set; }
        public List<DoctorTopMedicine> TopMedicines { get; set; } = new();
        public List<DoctorRecentPrescription> RecentPrescriptions { get; set; } = new();
        public List<DoctorActivityTrendPoint> ActivityTrend { get; set; } = new();
    }

    public class DoctorTopMedicine
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class DoctorRecentPrescription
    {
        public string PrescriptionId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string? Diagnosis { get; set; }
        public DateTime Date { get; set; }
    }

    public class DoctorActivityTrendPoint
    {
        public DateTime Period { get; set; }
        public int Count { get; set; }
    }

    public class DoctorDetailsResponse
    {
        public DoctorSummaryResponse Profile { get; set; } = new();
        public DoctorPerformanceStats Performance { get; set; } = new();
    }
}
