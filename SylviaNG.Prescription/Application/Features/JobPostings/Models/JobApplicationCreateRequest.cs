using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.JobPostings.Models
{
    public class JobApplicationCreateRequest
    {
        public long JobPostingId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string? CandidateEmail { get; set; }
        public string? CandidatePhone { get; set; }
        public string? ResumeUrl { get; set; }
        public string? CoverLetter { get; set; }
    }
}
