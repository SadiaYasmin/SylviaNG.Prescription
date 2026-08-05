namespace SylviaNG.Prescription.Application.Features.Doctors.Models
{
    /// <summary>
    /// Roster-wide stat tiles (US-054). TotalPrescriptions/TotalMedicineEntries are
    /// hardcoded to 0 — Prescription/Medicine entities don't exist yet (Epic D/F) — and
    /// should become real aggregate queries once those tables land, not before.
    /// </summary>
    public class DoctorListSummary
    {
        public int TotalDoctors { get; set; }
        public int ActiveDoctors { get; set; }
        public int TotalPrescriptions { get; set; }
        public int TotalMedicineEntries { get; set; }
    }

    public class DoctorListResponse
    {
        public List<DoctorSummaryResponse> Doctors { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public DoctorListSummary Summary { get; set; } = new();
    }
}
