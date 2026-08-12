using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Consultations.Models
{
    /// <summary>
    /// The minimal identifying summary of a consultation — used both for a freshly-created
    /// consultation and for the existing one surfaced back to the frontend when
    /// CreateConsultation finds a duplicate.
    /// </summary>
    public class ConsultationSummaryResponse
    {
        public long ConsultationId { get; set; }
        public string DisplayCode { get; set; } = string.Empty;
        public string TokenNumber { get; set; } = string.Empty;
        public ConsultationStatusEnum Status { get; set; }
    }
}
