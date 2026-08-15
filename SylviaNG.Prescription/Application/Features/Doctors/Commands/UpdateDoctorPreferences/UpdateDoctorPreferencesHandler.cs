using MediatR;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctorPreferences
{
    /// <summary>Epic K stub (US-064/065): a doctor picks their own preferred template/language. No admin path.</summary>
    public class UpdateDoctorPreferencesHandler : IRequestHandler<UpdateDoctorPreferencesCommand, DoctorPreferencesResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateDoctorPreferencesHandler(
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            ITemplateRepository templateRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _templateRepository = templateRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<DoctorPreferencesResponse> Handle(UpdateDoctorPreferencesCommand command, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                command.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var doctor = await _doctorRepository.GetByIdAsync(caller.DoctorId!.Value)
                ?? throw new NotFoundException("Doctor", caller.DoctorId!.Value);

            if (command.Request.PreferredTemplateId.HasValue)
            {
                var template = await _templateRepository.GetByIdAsync(command.Request.PreferredTemplateId.Value)
                    ?? throw new NotFoundException("PrescriptionTemplate", command.Request.PreferredTemplateId.Value);
                if (!template.Enabled)
                    throw new BadRequestException("Cannot prefer a disabled template.");
            }

            doctor.PreferredTemplateId = command.Request.PreferredTemplateId;
            doctor.PreferredLanguage = command.Request.PreferredLanguage;
            _doctorRepository.Update(doctor);
            await _unitOfWork.SaveChangesAsync();

            return new DoctorPreferencesResponse
            {
                PreferredTemplateId = doctor.PreferredTemplateId,
                SignatureUrl = doctor.SignatureUrl,
                PreferredLanguage = doctor.PreferredLanguage
            };
        }
    }
}
