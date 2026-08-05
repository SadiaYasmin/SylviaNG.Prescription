using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Audit;

namespace SylviaNG.Prescription.Domain.Entities;

/// <summary>
/// A doctor's professional profile (US-053). One-to-one with <see cref="User"/>,
/// which owns the Keycloak identity link, username, email, role, and active/inactive
/// status — those are never duplicated here to avoid a two-places-for-one-fact bug.
/// </summary>
public class Doctor : Audit
{
    public long DoctorId { get; set; }
    public long UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Qualification { get; set; }
    public string? Department { get; set; }
    public string? LicenseNumber { get; set; }
    public string? Specialization { get; set; }
    public int? ExperienceYears { get; set; }
    public GenderEnum? Gender { get; set; }
    public DateOnly? JoiningDate { get; set; }
    public string? PhotoBase64 { get; set; }

    public User? User { get; set; }
}
