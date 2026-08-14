namespace SylviaNG.Prescription.Application.Features.Prescriptions.Models
{
    /// <summary>
    /// The single authoring entry point (US-018/019/020), disambiguated by which id is set —
    /// exactly one of <see cref="ConsultationId"/> (open from queue), <see cref="PatientId"/>
    /// (quick-create walk-in, US-019), or <see cref="PrescriptionId"/> (resume a draft,
    /// US-020) is expected. <see cref="Force"/> proceeds past the US-011/012 guards after the
    /// caller has already seen and dismissed the prompt.
    /// </summary>
    public class StartOrResumePrescriptionRequest
    {
        public long? ConsultationId { get; set; }
        public long? PatientId { get; set; }
        public long? PrescriptionId { get; set; }
        public bool Force { get; set; }
    }
}
