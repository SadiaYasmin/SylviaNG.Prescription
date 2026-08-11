namespace SylviaNG.Prescription.Application.Features.HospitalSettings.Models
{
    public class HospitalSettingsResponse
    {
        public long HospitalSettingsId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LogoBase64 { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? EmergencyNumber { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? Slogan { get; set; }
        public string? SloganBn { get; set; }
        public string? LicenseNumber { get; set; }
        public string? SealBase64 { get; set; }
    }
}
