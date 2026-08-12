using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Pagination;

namespace SylviaNG.Prescription.Application.Features.Consultations.Models
{
    /// <summary>
    /// Admin listing filters (GetConsultationList). <see cref="PagedRequest.SearchTerm"/>
    /// matches patient name, token number, or patient phone. <see cref="Date"/> only applies
    /// when <see cref="DateMode"/> is Custom; <see cref="FromDate"/>/<see cref="ToDate"/>
    /// only apply when it's Range.
    /// </summary>
    public class ConsultationListRequest : PagedRequest
    {
        public ConsultationDateModeEnum DateMode { get; set; } = ConsultationDateModeEnum.Today;
        public DateOnly? Date { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public long? DoctorId { get; set; }
        public ConsultationStatusEnum? Status { get; set; }
    }
}
