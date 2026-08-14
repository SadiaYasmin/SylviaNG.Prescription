using MediatR;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctorSignature
{
    /// <summary>Epic K stub (US-026 unblocker): plain inline base64 upload, no AI background removal.</summary>
    public class UpdateDoctorSignatureHandler : IRequestHandler<UpdateDoctorSignatureCommand, DoctorPreferencesResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateDoctorSignatureHandler(
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<DoctorPreferencesResponse> Handle(UpdateDoctorSignatureCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.Request.SignatureBase64))
                throw new BadRequestException("Signature image is required.");

            var caller = await CallerContextResolver.ResolveCallerAsync(
                command.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var doctor = await _doctorRepository.GetByIdAsync(caller.DoctorId!.Value)
                ?? throw new NotFoundException("Doctor", caller.DoctorId!.Value);

            doctor.SignatureBase64 = command.Request.SignatureBase64;
            _doctorRepository.Update(doctor);
            await _unitOfWork.SaveChangesAsync();

            return new DoctorPreferencesResponse
            {
                PreferredTemplateId = doctor.PreferredTemplateId,
                SignatureBase64 = doctor.SignatureBase64,
                PreferredLanguage = doctor.PreferredLanguage
            };
        }
    }
}
