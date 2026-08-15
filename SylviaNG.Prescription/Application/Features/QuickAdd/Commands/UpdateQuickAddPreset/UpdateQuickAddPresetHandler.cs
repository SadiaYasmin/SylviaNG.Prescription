using MediatR;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.QuickAdd.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.QuickAdd.Commands.UpdateQuickAddPreset
{
    public class UpdateQuickAddPresetHandler : IRequestHandler<UpdateQuickAddPresetCommand, QuickAddPresetResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IQuickAddPresetRepository _quickAddPresetRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateQuickAddPresetHandler(
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            IQuickAddPresetRepository quickAddPresetRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _quickAddPresetRepository = quickAddPresetRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<QuickAddPresetResponse> Handle(UpdateQuickAddPresetCommand command, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                command.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var doctorId = caller.DoctorId!.Value;

            var preset = await _quickAddPresetRepository.GetByIdAsync(command.QuickAddPresetId)
                ?? throw new NotFoundException("QuickAddPreset", command.QuickAddPresetId);
            if (preset.DoctorId != doctorId)
                throw new NotFoundException("QuickAddPreset", command.QuickAddPresetId);

            preset.Label = command.Request.Label;
            preset.PayloadJson = command.Request.PayloadJson;

            _quickAddPresetRepository.Update(preset);
            await _unitOfWork.SaveChangesAsync();

            return preset.ToResponse();
        }
    }
}
