using SylviaNG.Prescription.SharedKernel.Pagination;

namespace SylviaNG.Prescription.Application.Features.Doctors.Models
{
    public class DoctorListRequest : PagedRequest
    {
        public string? Department { get; set; }
        public bool? IsActive { get; set; }
    }
}
