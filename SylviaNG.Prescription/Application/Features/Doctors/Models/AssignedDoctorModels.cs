namespace SylviaNG.Prescription.Application.Features.Doctors.Models
{
    /// <summary>A staff member's own read-only view of one assigned doctor, list row shape.</summary>
    public class AssignedDoctorListItem
    {
        public long DoctorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string Phone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class AssignedDoctorListResponse
    {
        public List<AssignedDoctorListItem> Doctors { get; set; } = new();
    }

    /// <summary>
    /// Single assigned doctor's details, scoped to exactly what a Staff user is allowed to see —
    /// deliberately not the full Admin <see cref="DoctorDetailsResponse"/> (no prescribing
    /// analytics, medicine breakdowns, etc.), just identity/contact fields plus two counts.
    /// </summary>
    public class AssignedDoctorDetailsResponse
    {
        public long DoctorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public string? Department { get; set; }
        public string? Email { get; set; }
        public string Phone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? PhotoUrl { get; set; }
        /// <summary>
        /// When this doctor was assigned to the calling staff member. Null for assignments made
        /// before this field started being populated — StaffDoctor.CreatedAt is only stamped on
        /// creation going forward, there is no backfill for pre-existing rows.
        /// </summary>
        public DateTime? AssignedDate { get; set; }
        public int TodayAppointments { get; set; }
        /// <summary>Completed consultations for THIS doctor today only, not all-time.</summary>
        public int CompletedConsultations { get; set; }
    }
}
