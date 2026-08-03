namespace SylviaNG.Prescription.Application.Features.JobPostings.Models
{
    public class JobPostingLookupResponse
    {
        public long JobPostingId { get; set; }
        public string? Title { get; set; }

        public string CodeName =>
            (!string.IsNullOrWhiteSpace(Title))
                ? $"{JobPostingId} - {Title}"
                : Title ?? string.Empty;
    }
}
