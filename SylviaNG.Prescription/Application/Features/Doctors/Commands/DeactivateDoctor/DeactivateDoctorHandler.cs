using MediatR;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Interfaces.Externals;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Doctors.Commands.DeactivateDoctor
{
    public class DeactivateDoctorHandler : IRequestHandler<DeactivateDoctorCommand, Unit>
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUserRepository _userRepository;
        private readonly IKeycloakAdminClient _adminClient;
        private readonly IUnitOfWork _unitOfWork;

        public DeactivateDoctorHandler(
            IDoctorRepository doctorRepository,
            IUserRepository userRepository,
            IKeycloakAdminClient adminClient,
            IUnitOfWork unitOfWork)
        {
            _doctorRepository = doctorRepository;
            _userRepository = userRepository;
            _adminClient = adminClient;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(DeactivateDoctorCommand command, CancellationToken cancellationToken)
        {
            var doctor = await _doctorRepository.GetByIdAsync(command.DoctorId)
                ?? throw new NotFoundException("Doctor", command.DoctorId);
            var user = await _userRepository.GetByIdAsync(doctor.UserId)
                ?? throw new NotFoundException("User", doctor.UserId);

            if (!user.IsActive)
                return Unit.Value;

            await _adminClient.SetUserEnabledAsync(user.KeycloakId, false);
            user.IsActive = false;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
