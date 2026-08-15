using SylviaNG.Prescription.SharedKernel.Audit;

namespace SylviaNG.Prescription.Domain.Entities;

/// <summary>
/// Singleton hospital identity/branding row (Epic H / US-045), used automatically across every
/// prescription template. Exactly one row is guaranteed to exist by <c>TemplateEngineSeeder</c> —
/// callers fetch it via <c>.FirstOrDefaultAsync()</c>, never a hardcoded id.
/// </summary>
public class HospitalSettings : Audit
{
    public long HospitalSettingsId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? EmergencyNumber { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Slogan { get; set; }
    public string? SloganBn { get; set; }
    public string? LicenseNumber { get; set; }
    public string? SealUrl { get; set; }
}
