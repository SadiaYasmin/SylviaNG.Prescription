using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.Infrastructure.Data;
using static SylviaNG.Prescription.Application.Common.CallerContextResolver;

namespace SylviaNG.Prescription.Application.Features.Prescriptions
{
    /// <summary>
    /// Filters which <see cref="PrescriptionRecord"/> rows a caller may see (Epic D/E),
    /// same shape as <see cref="Patients.PatientVisibilityScope"/>. Two scopes exist because
    /// the spec draws a real distinction: US-029/030's "my drafts"/"my finalized" lists are
    /// strictly doctor-own (<paramref name="ownOnly"/> = true), while US-031's single-
    /// prescription view and US-032's patient-history panel are broader — Admin sees all,
    /// Doctor also sees any prescription for a patient in their care team (not just ones they
    /// personally authored, for genuine cross-doctor audit/continuity), Staff sees
    /// prescriptions for patients they personally registered.
    /// </summary>
    public static class PrescriptionVisibilityScope
    {
        public static async Task<IQueryable<PrescriptionRecord>> ApplyAsync(
            IQueryable<PrescriptionRecord> prescriptions,
            ApplicationDBContext context,
            CallerContext caller,
            bool ownOnly = false,
            CancellationToken cancellationToken = default)
        {
            switch (caller.Role)
            {
                case UserRoleEnum.Admin:
                    return prescriptions;

                case UserRoleEnum.Doctor:
                    if (ownOnly)
                        return prescriptions.Where(p => p.DoctorId == caller.DoctorId);

                    var assignedStaffIds = await context.StaffDoctors
                        .Where(sd => sd.DoctorId == caller.DoctorId)
                        .Select(sd => sd.StaffId)
                        .ToListAsync(cancellationToken);
                    var careTeamPatientIds = await context.Patients
                        .Where(p => assignedStaffIds.Contains(p.RegisteredByStaffId))
                        .Select(p => p.PatientId)
                        .ToListAsync(cancellationToken);
                    return prescriptions.Where(p => p.DoctorId == caller.DoctorId || careTeamPatientIds.Contains(p.PatientId));

                case UserRoleEnum.Staff:
                    var ownPatientIds = await context.Patients
                        .Where(p => p.RegisteredByStaffId == caller.StaffId)
                        .Select(p => p.PatientId)
                        .ToListAsync(cancellationToken);
                    return prescriptions.Where(p => ownPatientIds.Contains(p.PatientId));

                default:
                    return prescriptions.Where(_ => false);
            }
        }

        public static async Task<bool> IsVisibleAsync(
            PrescriptionRecord prescription,
            ApplicationDBContext context,
            CallerContext caller,
            bool ownOnly = false,
            CancellationToken cancellationToken = default)
        {
            var scoped = await ApplyAsync(new[] { prescription }.AsQueryable(), context, caller, ownOnly, cancellationToken);
            return scoped.Any();
        }
    }
}
