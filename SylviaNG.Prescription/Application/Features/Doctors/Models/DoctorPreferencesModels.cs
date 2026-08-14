using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Doctors.Models
{
    /// <summary>
    /// Epic K stub — just the 3 self-service fields Prescription authoring/finalize (Epic D)
    /// needs (US-026/064/065). Full profile self-service (photo, license, contact info) and
    /// AI signature background-removal are still Epic K's own job.
    /// </summary>
    public class DoctorPreferencesResponse
    {
        public long? PreferredTemplateId { get; set; }
        public string? SignatureBase64 { get; set; }
        public TemplateLanguageEnum? PreferredLanguage { get; set; }
    }

    public class UpdateDoctorPreferencesRequest
    {
        public long? PreferredTemplateId { get; set; }
        public TemplateLanguageEnum? PreferredLanguage { get; set; }
    }

    public class UpdateDoctorSignatureRequest
    {
        /// <summary>Plain inline base64 image, same storage pattern as Doctor.PhotoBase64 — no background removal.</summary>
        public string SignatureBase64 { get; set; } = string.Empty;
    }
}
