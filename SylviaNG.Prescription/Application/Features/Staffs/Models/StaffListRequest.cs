using SylviaNG.Prescription.SharedKernel.Pagination;

namespace SylviaNG.Prescription.Application.Features.Staffs.Models
{
    public class StaffListRequest : PagedRequest
    {
        public string? Department { get; set; }
        public bool? IsActive { get; set; }
    }
}
