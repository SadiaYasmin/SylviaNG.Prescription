namespace SylviaNG.Prescription.Application.Features.Doctors.Models
{
    /// <summary>
    /// Roster-wide stat tiles (US-054). TotalPrescriptions is the count of all finalized
    /// prescriptions; TotalMedicineEntries is the sum of medicine line items across those
    /// prescriptions (same convention as <see cref="DoctorPerformanceStats.TotalMedicinesPrescribed"/>
    /// on the per-doctor drill-down) — not the medicine catalog row count.
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
