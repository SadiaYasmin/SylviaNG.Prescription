using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.JobPostings.Models
{
    public class JobApplicationResponse
    {
        public long JobApplicationId { get; set; }
        public long JobPostingId { get; set; }
        public string? JobPostingTitle { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string? CandidateEmail { get; set; }
        public string? CandidatePhone { get; set; }
        public string? ResumeUrl { get; set; }
        public ApplicationStatusEnum ApplicationStatus { get; set; }
        public DateTime? AppliedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
