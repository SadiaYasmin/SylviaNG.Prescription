using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Common
{
    /// <summary>
    /// Resolves "who is calling" from the JWT's Keycloak subject, mirroring the
    /// KeycloakId → User → Staff/Doctor resolution originally introduced in
    /// <c>GetAssignedStaffHandler</c> (Epic J) and generalized here (Epic C) so every
    /// feature that needs the caller's role/StaffId/DoctorId — not just Patients — shares
    /// exactly one implementation instead of duplicating it per feature.
    /// </summary>
    public static class CallerContextResolver
    {
        /// <summary>
        /// The caller's resolved identity. Exactly one of <see cref="StaffId"/>/<see cref="DoctorId"/>
        /// is set, depending on <see cref="Role"/> (neither is set for Admin).
        /// </summary>
        public readonly record struct CallerContext(UserRoleEnum Role, long? StaffId, long? DoctorId);

        public static async Task<CallerContext> ResolveCallerAsync(
            string keycloakId,
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository)
        {
            var user = await userRepository.GetByKeycloakIdAsync(keycloakId)
                ?? throw new NotFoundException("User", keycloakId);

            switch (user.Role)
            {
                case UserRoleEnum.Staff:
                    var staff = await staffRepository.GetByUserIdAsync(user.UserId)
                        ?? throw new NotFoundException("Staff", user.UserId);
                    return new CallerContext(UserRoleEnum.Staff, staff.StaffId, null);

                case UserRoleEnum.Doctor:
                    var doctor = await doctorRepository.GetByUserIdAsync(user.UserId)
                        ?? throw new NotFoundException("Doctor", user.UserId);
                    return new CallerContext(UserRoleEnum.Doctor, null, doctor.DoctorId);

                default:
                    return new CallerContext(UserRoleEnum.Admin, null, null);
            }
        }
    }
}
