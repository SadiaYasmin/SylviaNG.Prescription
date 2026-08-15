namespace SylviaNG.Prescription.Application.Features.Analytics.Models
{
    /// <summary>US-073 leaderboard row. Every active doctor appears, zero-valued if inactive on this metric.</summary>
    public class DoctorLeaderboardEntry
    {
        public long DoctorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int PatientsConsulted { get; set; }
        public int PrescriptionsCreated { get; set; }
        public int MedicinesPrescribed { get; set; }
        public double AvgRxPerConsultation { get; set; }
        public double AvgMedsPerRx { get; set; }
    }
}
