using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Patients.Models
{
    /// <summary>
    /// One row of a doctor's patient queue — the same fields as
    /// <see cref="PatientSummaryResponse"/> plus today's per-patient consultation state, so
    /// the frontend can pick the right row action (Start / Continue / View) without a second
    /// round trip. <see cref="TodayConsultationStatus"/> is null when the patient has no
    /// consultation record for today at all. <see cref="TodayPrescriptionId"/> is only ever
    /// set when <see cref="TodayConsultationStatus"/> is <see cref="ConsultationStatusEnum.Completed"/>.
    /// </summary>
    public class DoctorPatientQueueItemResponse : PatientSummaryResponse
    {
        public long? TodayConsultationId { get; set; }
        public ConsultationStatusEnum? TodayConsultationStatus { get; set; }
        public long? TodayPrescriptionId { get; set; }
    }
}
