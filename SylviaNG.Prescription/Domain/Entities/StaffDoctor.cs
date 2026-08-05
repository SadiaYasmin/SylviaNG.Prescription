using SylviaNG.Prescription.SharedKernel.Audit;

namespace SylviaNG.Prescription.Domain.Entities;

/// <summary>
/// Join row for the many-to-many assignment between a <see cref="Staff"/> member and
/// the <see cref="Doctor"/>(s) they support (US-057). A staff member can be assigned to
/// more than one doctor, and a doctor can have more than one staff member. Rows here are
/// never removed when a Staff/Doctor is deactivated — they're the historical record of
/// who supported whom, matching the no-orphaning principle used for soft-deletes elsewhere.
/// </summary>
public class StaffDoctor : Audit
{
    public long StaffDoctorId { get; set; }
    public long StaffId { get; set; }
    public long DoctorId { get; set; }

    public Staff? Staff { get; set; }
    public Doctor? Doctor { get; set; }
}
