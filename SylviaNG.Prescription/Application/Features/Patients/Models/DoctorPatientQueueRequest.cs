using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Patients.Models
{
    /// <summary>
    /// Filters for GetDoctorPatientQueue (Create Prescription's patient picker). Unpaged,
    /// same reasoning as GetTodaysQueue/GetMyQueue — one doctor's staff-scoped roster is
    /// expected to stay small enough to render in full.
    /// </summary>
    public class DoctorPatientQueueRequest
    {
        public PatientQueueFilterEnum QueueFilter { get; set; } = PatientQueueFilterEnum.TodayQueue;
        public string? SearchTerm { get; set; }
    }
}
