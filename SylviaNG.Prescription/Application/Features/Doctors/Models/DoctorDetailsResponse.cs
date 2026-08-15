namespace SylviaNG.Prescription.Application.Features.Doctors.Models
{
    /// <summary>
    /// Per-doctor performance drill-down (US-055, filled in for real by Epic M/US-073 now
    /// that Consultation/Prescription exist — see <c>GetDoctorDetailsHandler</c>).
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

        /// <summary>
        /// US-073's busiest-consultation-hours histogram — always 24 entries (Hour 0–23),
        /// bucketed off <see cref="SylviaNG.Prescription.Domain.Entities.Consultation.CheckInAt"/>
        /// (never <c>Audit.CreatedAt</c>, which is never populated anywhere in this codebase).
        /// Reported in UTC hour-of-day — there is no hospital-timezone setting to convert to.
        /// </summary>
        public List<HourBucket> BusiestHours { get; set; } = new();
    }

    public class HourBucket
    {
        public int Hour { get; set; }
        public int Count { get; set; }
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
