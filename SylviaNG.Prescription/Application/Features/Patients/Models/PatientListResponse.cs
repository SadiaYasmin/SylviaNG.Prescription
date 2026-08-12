namespace SylviaNG.Prescription.Application.Features.Patients.Models
{
    public class PatientListResponse
    {
        public List<PatientSummaryResponse> Patients { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
