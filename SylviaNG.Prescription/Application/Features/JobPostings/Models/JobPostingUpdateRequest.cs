using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.JobPostings.Models
{
    public class JobPostingUpdateRequest
    {
        public long? DepartmentId { get; set; }
        public long? DesignationId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Requirements { get; set; }
        public int? NumberOfPositions { get; set; }
        public EmploymentTypeEnum? EmploymentType { get; set; }
        public JobStatusEnum? Status { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public DateTime? PostingDate { get; set; }
        public DateTime? ClosingDate { get; set; }
        public bool? IsActive { get; set; }
    }
}
