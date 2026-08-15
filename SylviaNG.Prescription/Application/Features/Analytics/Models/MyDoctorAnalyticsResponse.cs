namespace SylviaNG.Prescription.Application.Features.Analytics.Models
{
    /// <summary>US-077: a doctor's own scoped stats.</summary>
    public class MyDoctorAnalyticsResponse
    {
        public int OwnPatientCount { get; set; }
        public int PatientsConsulted { get; set; }
        public int DraftPrescriptionCount { get; set; }
        public int FinalizedPrescriptionCount { get; set; }
        public int AssignedStaffCount { get; set; }
        public List<MedicineCountEntry> TopMedicines { get; set; } = new();
    }
}
