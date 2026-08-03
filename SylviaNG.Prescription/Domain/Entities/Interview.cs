using SylviaNG.Prescription.SharedKernel.Audit;

namespace SylviaNG.Prescription.Domain.Entities;

/// <summary>
/// Represents an interview scheduled for a job application.
/// </summary>
public class Interview : Audit
{
    public long InterviewId { get; set; }
    public long JobApplicationId { get; set; }
    public long? InterviewerId { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public string? Location { get; set; }
    public string? MeetingLink { get; set; }
    public string? Round { get; set; }
    public string? Feedback { get; set; }
    public string? Result { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public JobApplication JobApplication { get; set; } = null!;
}
