using MediatR;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Interfaces.Externals;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Interfaces.Services;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctor
{
    public class UpdateDoctorHandler : IRequestHandler<UpdateDoctorCommand, DoctorSummaryResponse>
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUserRepository _userRepository;
        private readonly IKeycloakAdminClient _adminClient;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateDoctorHandler(
            IDoctorRepository doctorRepository,
            IUserRepository userRepository,
            IKeycloakAdminClient adminClient,
            IFileStorageService fileStorageService,
            IUnitOfWork unitOfWork)
        {
            _doctorRepository = doctorRepository;
            _userRepository = userRepository;
            _adminClient = adminClient;
            _fileStorageService = fileStorageService;
            _unitOfWork = unitOfWork;
        }

        public async Task<DoctorSummaryResponse> Handle(UpdateDoctorCommand command, CancellationToken cancellationToken)
        {
            var doctor = await _doctorRepository.GetByIdAsync(command.DoctorId)
                ?? throw new NotFoundException("Doctor", command.DoctorId);
            var user = await _userRepository.GetByIdAsync(doctor.UserId)
                ?? throw new NotFoundException("User", doctor.UserId);

            var request = command.Request;

            if (!string.IsNullOrWhiteSpace(request.LicenseNumber) &&
                await _doctorRepository.ExistsByLicenseNumberAsync(request.LicenseNumber, doctor.DoctorId))
            {
                throw new DuplicateException("Doctor", "LicenseNumber", request.LicenseNumber);
            }

            doctor.FullName = request.FullName;
            doctor.Phone = request.Phone;
            doctor.Qualification = request.Qualification;
            doctor.Department = request.Department;
            doctor.LicenseNumber = request.LicenseNumber;
            doctor.Specialization = request.Specialization;
            doctor.ExperienceYears = request.ExperienceYears;
            doctor.Gender = request.Gender;
            doctor.JoiningDate = request.JoiningDate;

            // US-083: a null PhotoBase64 leaves the current photo untouched (the edit form
            // only carries a value when the admin actually picked a new file) — an empty
            // string is the explicit "remove photo" signal, matching the button's intent.
            if (request.PhotoBase64 is not null)
            {
                await _fileStorageService.DeleteAsync(doctor.PhotoUrl);
                doctor.PhotoUrl = await _fileStorageService.SaveImageAsync(request.PhotoBase64, "doctor-photos", cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
                user.Email = request.Email;

            if (user.IsActive != request.IsActive)
            {
                await _adminClient.SetUserEnabledAsync(user.KeycloakId, request.IsActive);
                user.IsActive = request.IsActive;
            }

            _doctorRepository.Update(doctor);
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return doctor.ToSummaryResponse(user);
        }
    }
}
