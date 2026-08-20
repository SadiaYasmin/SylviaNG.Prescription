using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Commands.AutoSavePrescription
{
    /// <summary>Lightweight acknowledgement for an auto-save — no full document round-trip.</summary>
    public class AutoSavePrescriptionResponse
    {
        public long PrescriptionId { get; set; }
        public PrescriptionStatusEnum Status { get; set; }
        public DateTime AutoSavedAt { get; set; }
    }
}
