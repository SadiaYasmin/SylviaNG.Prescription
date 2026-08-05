namespace SylviaNG.Prescription.Application.Features.Staffs.Models
{
    public class StaffListResponse
    {
        public List<StaffSummaryResponse> Staff { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
