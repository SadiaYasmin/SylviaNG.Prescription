using MediatR;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Interfaces.Externals;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Staffs.Commands.DeactivateStaff
{
    public class DeactivateStaffHandler : IRequestHandler<DeactivateStaffCommand, Unit>
    {
        private readonly IStaffRepository _staffRepository;
        private readonly IUserRepository _userRepository;
        private readonly IKeycloakAdminClient _adminClient;
        private readonly IUnitOfWork _unitOfWork;

        public DeactivateStaffHandler(
            IStaffRepository staffRepository,
            IUserRepository userRepository,
            IKeycloakAdminClient adminClient,
            IUnitOfWork unitOfWork)
        {
            _staffRepository = staffRepository;
            _userRepository = userRepository;
            _adminClient = adminClient;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(DeactivateStaffCommand command, CancellationToken cancellationToken)
        {
            var staff = await _staffRepository.GetByIdAsync(command.StaffId)
                ?? throw new NotFoundException("Staff", command.StaffId);
            var user = await _userRepository.GetByIdAsync(staff.UserId)
                ?? throw new NotFoundException("User", staff.UserId);

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
