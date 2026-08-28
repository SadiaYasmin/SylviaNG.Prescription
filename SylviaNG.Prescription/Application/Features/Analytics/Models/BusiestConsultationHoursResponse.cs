namespace SylviaNG.Prescription.Application.Features.Analytics.Models
{
    /// <summary>
    /// Hospital-wide "Busiest Consultation Hours" for the Analytics dashboard (US-073) —
    /// aggregates <see cref="SylviaNG.Prescription.Domain.Entities.Consultation.CheckInAt"/>
    /// across every doctor (unlike <c>GetDoctorDetailsHandler</c>'s per-doctor histogram) and
    /// reports it in Bangladesh Time (UTC+6, no DST) since the hospital operates in Dhaka.
    /// Always 24 entries (Hour 0-23).
    /// </summary>
    public class BusiestConsultationHoursResponse
    {
        public List<HourBucket> Hours { get; set; } = new();
    }

    public class HourBucket
    {
        public int Hour { get; set; }
        public int Count { get; set; }
    }
}
