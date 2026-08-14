using SylviaNG.Prescription.Application.Features.Templates.Models;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Models
{
    /// <summary>
    /// Everything a frontend needs to render a prescription in one call — authoring,
    /// read-only view, or public verify — so none of those three surfaces needs a second
    /// protected-endpoint round trip just to resolve the template/hospital branding.
    /// Reused by StartOrResumePrescription, GetPrescriptionDetails, and VerifyPrescription.
    /// </summary>
    public class PrescriptionDocumentResponse
    {
        public long PrescriptionId { get; set; }
        public string DisplayCode { get; set; } = string.Empty;
        public long ConsultationId { get; set; }
        public PrescriptionStatusEnum Status { get; set; }
        public TemplateLanguageEnum Language { get; set; }
        public DateTime? SavedAt { get; set; }
        public DateTime? FinalizedAt { get; set; }

        public PrescriptionContent Content { get; set; } = new();

        public PatientSnapshotResponse Patient { get; set; } = new();
        public DoctorSnapshotResponse Doctor { get; set; } = new();

        public long TemplateId { get; set; }
        public TemplateTypeEnum TemplateType { get; set; }
        public TemplateConfig TemplateConfig { get; set; } = new();

        public HospitalSettingsSnapshotResponse HospitalSettings { get; set; } = new();
    }

    /// <summary>Just the branding fields the templates render — not HospitalSettings' full CRUD shape.</summary>
    public class HospitalSettingsSnapshotResponse
    {
        public string Name { get; set; } = string.Empty;
        public string? LogoBase64 { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? EmergencyNumber { get; set; }
        public string? Website { get; set; }
        public string? Slogan { get; set; }
        public string? SloganBn { get; set; }
        public string? LicenseNumber { get; set; }
        public string? SealBase64 { get; set; }
    }
}
