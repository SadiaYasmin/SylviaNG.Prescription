using MediatR;

namespace SylviaNG.Prescription.Application.Features.Doctors.Commands.DeactivateDoctor
{
    /// <summary>
    /// US-056 "delete a doctor account" — implemented as a soft-delete (deactivate),
    /// not a hard row delete, per feature.md's explicit recommendation: hard-deleting
    /// must not silently orphan or corrupt a doctor's historical prescriptions/consultations.
    /// </summary>
    public class DeactivateDoctorCommand : IRequest<Unit>
    {
        public long DoctorId { get; set; }

        public DeactivateDoctorCommand(long doctorId)
        {
            DoctorId = doctorId;
        }
    }
}
