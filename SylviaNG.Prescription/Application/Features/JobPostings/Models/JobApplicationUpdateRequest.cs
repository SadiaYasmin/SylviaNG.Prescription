using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.JobPostings.Models
{
    public class JobApplicationUpdateRequest
    {
        public ApplicationStatusEnum? ApplicationStatus { get; set; }
        public string? CandidatePhone { get; set; }
        public string? ResumeUrl { get; set; }
        public string? CoverLetter { get; set; }
        public bool? IsActive { get; set; }
    }
}
