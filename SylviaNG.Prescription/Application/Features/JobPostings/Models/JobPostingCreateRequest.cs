using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.JobPostings.Models
{
    public class JobPostingCreateRequest
    {
        public long SiteId { get; set; }
        public long? DepartmentId { get; set; }
        public long? DesignationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Requirements { get; set; }
        public int NumberOfPositions { get; set; } = 1;
        public EmploymentTypeEnum EmploymentType { get; set; } = EmploymentTypeEnum.FullTime;
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public DateTime? PostingDate { get; set; }
        public DateTime? ClosingDate { get; set; }
    }
}
