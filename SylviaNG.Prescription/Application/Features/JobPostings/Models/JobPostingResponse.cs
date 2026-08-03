using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.JobPostings.Models
{
    public class JobPostingResponse
    {
        public long JobPostingId { get; set; }
        public long SiteId { get; set; }
        public string? SiteName { get; set; }
        public long? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public long? DesignationId { get; set; }
        public string? DesignationName { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Requirements { get; set; }
        public int NumberOfPositions { get; set; }
        public EmploymentTypeEnum EmploymentType { get; set; }
        public JobStatusEnum Status { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public DateTime? PostingDate { get; set; }
        public DateTime? ClosingDate { get; set; }
        public bool IsActive { get; set; }
        public int TotalApplications { get; set; }
    }
}
