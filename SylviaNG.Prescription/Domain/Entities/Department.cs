using SylviaNG.Prescription.SharedKernel.Audit;

namespace SylviaNG.Prescription.Domain.Entities
{
    /// <summary>
    /// Admin-managed department list (e.g. "General Medicine", "Cardiology") used as the
    /// selectable options for Doctor.Department / Staff.Department. Deliberately not a
    /// foreign key on Doctor/Staff — those remain plain name strings so existing records are
    /// never invalidated by a rename here; this table only drives the dropdown's option list.
    /// </summary>
    public class Department : Audit
    {
        public long DepartmentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
