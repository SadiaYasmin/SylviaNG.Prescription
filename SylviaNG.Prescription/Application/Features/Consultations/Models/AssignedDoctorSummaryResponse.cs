namespace SylviaNG.Prescription.Application.Features.Consultations.Models
{
    /// <summary>
    /// Doctor picker entry for the create-consultation flow (GetMyAssignedDoctors). Kept in
    /// the Consultations feature — not reused from Epic J's Staffs.Models.AssignedDoctorSummary
    /// — since it exists specifically to support this create-consultation doctor picker and
    /// has no reason to couple to the already-shipped Staffs feature.
    /// </summary>
    public class AssignedDoctorSummaryResponse
    {
        public long DoctorId { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}
