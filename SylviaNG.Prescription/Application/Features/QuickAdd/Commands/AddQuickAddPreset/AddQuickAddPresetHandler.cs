using MediatR;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Features.QuickAdd.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.QuickAdd.Commands.AddQuickAddPreset
{
    public class AddQuickAddPresetHandler : IRequestHandler<AddQuickAddPresetCommand, QuickAddPresetResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IQuickAddPresetRepository _quickAddPresetRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddQuickAddPresetHandler(
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

        public async Task<QuickAddPresetResponse> Handle(AddQuickAddPresetCommand command, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                command.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var doctorId = caller.DoctorId!.Value;

            var preset = new QuickAddPreset
            {
                DoctorId = doctorId,
                SectionType = command.Request.SectionType,
                Label = command.Request.Label,
                PayloadJson = command.Request.PayloadJson
            };

            await _quickAddPresetRepository.AddAsync(preset);
            await _unitOfWork.SaveChangesAsync();

            return preset.ToResponse();
        }
    }
}
