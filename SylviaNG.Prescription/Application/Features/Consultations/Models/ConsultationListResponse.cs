namespace SylviaNG.Prescription.Application.Features.Consultations.Models
{
    /// <summary>
    /// Stat-tile counts (Total/Waiting/InProgress/Completed/Draft) computed over the FILTERED
    /// set (date/doctor/status/search all applied) — not global roster counts — matching the
    /// reference prototype's behavior. "InProgress" maps to ConsultationStatusEnum.InConsultation.
    /// "Draft" (Epic D) is a consultation whose prescription was saved as a draft — out of the
    /// live queue but not yet completed.
    /// </summary>
    public class ConsultationListSummary
    {
        public int Total { get; set; }
        public int Waiting { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public int Draft { get; set; }
    }

    public class ConsultationListResponse
    {
        public List<ConsultationListItemResponse> Consultations { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public ConsultationListSummary Summary { get; set; } = new();
    }
}
