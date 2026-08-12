using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.Infrastructure.Data;
using static SylviaNG.Prescription.Application.Common.CallerContextResolver;

namespace SylviaNG.Prescription.Application.Features.Patients
{
    /// <summary>
    /// Filters which <see cref="Patient"/> rows a caller may see or act on (Epic B). Shared
    /// by UpdatePatient, GetPatientList, and GetPatientDetails so the Staff/Doctor visibility
    /// join lives in exactly one place instead of three: Admin sees everyone; a Staff member
    /// sees only patients they registered (<see cref="Patient.RegisteredByStaffId"/>); a Doctor
    /// sees patients registered by any Staff member assigned to them via
    /// <see cref="StaffDoctor"/> (Epic J's join table).
    ///
    /// Identity resolution itself (KeycloakId → User → Staff/Doctor) has moved to the shared
    /// <see cref="CallerContextResolver"/> (Epic C) so features outside Patients can reuse it
    /// too — this class keeps only the Patient-specific row-scoping.
    /// </summary>
    public static class PatientVisibilityScope
    {
        /// <summary>
        /// Filters a Patient query down to the rows visible to <paramref name="caller"/>.
        /// The Doctor branch materializes the assigned StaffIds first (rather than a nested
        /// subquery), matching the fetch-ids-then-Contains style already used for
        /// StaffDoctor joins elsewhere in this codebase (see GetStaffListHandler).
        /// </summary>
        public static async Task<IQueryable<Patient>> ApplyAsync(
            IQueryable<Patient> patients,
            ApplicationDBContext context,
            CallerContext caller,
            CancellationToken cancellationToken = default)
        {
            switch (caller.Role)
            {
                case UserRoleEnum.Admin:
                    return patients;

                case UserRoleEnum.Staff:
                    return patients.Where(p => p.RegisteredByStaffId == caller.StaffId);

                case UserRoleEnum.Doctor:
                    var assignedStaffIds = await context.StaffDoctors
                        .Where(sd => sd.DoctorId == caller.DoctorId)
                        .Select(sd => sd.StaffId)
                        .ToListAsync(cancellationToken);
                    return patients.Where(p => assignedStaffIds.Contains(p.RegisteredByStaffId));

                default:
                    return patients.Where(_ => false);
            }
        }

        /// <summary>
        /// Convenience check for a single already-loaded patient (used by UpdatePatient and
        /// GetPatientDetails, which fetch by id first). Reuses <see cref="ApplyAsync"/> rather
        /// than re-implementing the same role branches against one row.
        /// </summary>
        public static async Task<bool> IsVisibleAsync(
            Patient patient,
            ApplicationDBContext context,
            CallerContext caller,
            CancellationToken cancellationToken = default)
        {
            var scoped = await ApplyAsync(new[] { patient }.AsQueryable(), context, caller, cancellationToken);
            return scoped.Any();
        }
    }
}
