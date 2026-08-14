namespace SylviaNG.Prescription.Application.Features.Prescriptions.Models
{
    /// <summary>
    /// The subset of a Doctor needed to render the template's doctor-info/signature block,
    /// plus the two Epic K-stub fields the finalize checklist (US-026) cares about.
    /// </summary>
    public class DoctorSnapshotResponse
    {
        public long DoctorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Qualification { get; set; }
        public string? Department { get; set; }
        public string? LicenseNumber { get; set; }
        public string? SignatureBase64 { get; set; }
        public long? PreferredTemplateId { get; set; }
    }
}
