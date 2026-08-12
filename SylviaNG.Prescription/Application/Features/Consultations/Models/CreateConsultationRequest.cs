namespace SylviaNG.Prescription.Application.Features.Consultations.Models
{
    public class CreateConsultationRequest
    {
        public long PatientId { get; set; }
        public long DoctorId { get; set; }

        /// <summary>Null defaults to today (local/regional "today", see DateTimeUtility).</summary>
        public DateOnly? VisitDate { get; set; }

        /// <summary>
        /// When true, create even if a Waiting/InConsultation consultation already exists
        /// for the same (PatientId, DoctorId, VisitDate) — set after the frontend prompts
        /// the user with the duplicate found by a first, non-forced call.
        /// </summary>
        public bool Force { get; set; }
    }
}
