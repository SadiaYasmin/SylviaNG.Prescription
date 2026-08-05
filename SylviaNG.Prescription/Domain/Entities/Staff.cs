using SylviaNG.Prescription.SharedKernel.Audit;

namespace SylviaNG.Prescription.Domain.Entities;

/// <summary>
/// A staff member's profile (US-057). One-to-one with <see cref="User"/>, which owns
/// the Keycloak identity link, username, email, role, and active/inactive status —
/// those are never duplicated here to avoid a two-places-for-one-fact bug. A staff
/// member's doctor coverage is many-to-many, tracked via <see cref="StaffDoctor"/>.
/// </summary>
public class Staff : Audit
{
    public long StaffId { get; set; }
    public long UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Department { get; set; }

    public User? User { get; set; }
    public ICollection<StaffDoctor> StaffDoctors { get; set; } = new List<StaffDoctor>();
}
