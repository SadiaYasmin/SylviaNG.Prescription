using MediatR;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Interfaces.Services;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctorSignature
{
    /// <summary>Epic K stub (US-026 unblocker): plain inline base64 upload, no AI background removal.</summary>
    public class UpdateDoctorSignatureHandler : IRequestHandler<UpdateDoctorSignatureCommand, DoctorPreferencesResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateDoctorSignatureHandler(
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            IFileStorageService fileStorageService,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _fileStorageService = fileStorageService;
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

            await _fileStorageService.DeleteAsync(doctor.SignatureUrl);
            doctor.SignatureUrl = await _fileStorageService.SaveImageAsync(
                command.Request.SignatureBase64, "doctor-signatures", cancellationToken);
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
