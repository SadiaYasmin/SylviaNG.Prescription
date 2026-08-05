using MediatR;

namespace SylviaNG.Prescription.Application.Features.Staffs.Commands.DeactivateStaff
{
    /// <summary>
    /// US-060 "delete a staff account" — implemented as a soft-delete (deactivate),
    /// not a hard row delete, matching Doctor Management's convention: hard-deleting
    /// must not silently orphan or corrupt historical StaffDoctor assignment records.
    /// </summary>
    public class DeactivateStaffCommand : IRequest<Unit>
    {
        public long StaffId { get; set; }

        public DeactivateStaffCommand(long staffId)
        {
            StaffId = staffId;
        }
    }
}
