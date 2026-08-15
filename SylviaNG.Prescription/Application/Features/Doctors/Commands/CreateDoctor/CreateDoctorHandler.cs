using MediatR;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Auth.Models;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Interfaces.Services;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Doctors.Commands.CreateDoctor
{
    public class CreateDoctorHandler : IRequestHandler<CreateDoctorCommand, CreateDoctorResponse>
    {
        private readonly IAuthService _authService;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUnitOfWork _unitOfWork;

        public CreateDoctorHandler(
            IAuthService authService,
            IDoctorRepository doctorRepository,
            IFileStorageService fileStorageService,
            IUnitOfWork unitOfWork)
        {
            _authService = authService;
            _doctorRepository = doctorRepository;
            _fileStorageService = fileStorageService;
            _unitOfWork = unitOfWork;
        }

        public async Task<CreateDoctorResponse> Handle(CreateDoctorCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            if (!string.IsNullOrWhiteSpace(request.LicenseNumber) &&
                await _doctorRepository.ExistsByLicenseNumberAsync(request.LicenseNumber))
            {
                throw new DuplicateException("Doctor", "LicenseNumber", request.LicenseNumber);
            }

            var (firstName, lastName) = SplitName(request.FullName);
            var account = await _authService.CreateUserAccountAsync(new CreateUserAccountRequest
            {
                Username = request.Username,
                Email = request.Email,
                FirstName = firstName,
                LastName = lastName,
                Role = UserRoleEnum.Doctor
            });

            var doctor = new Doctor
            {
                UserId = account.UserId,
                FullName = request.FullName,
                Phone = request.Phone,
                Qualification = request.Qualification,
                Department = request.Department,
                LicenseNumber = request.LicenseNumber,
                Specialization = request.Specialization,
                ExperienceYears = request.ExperienceYears,
                Gender = request.Gender,
                JoiningDate = request.JoiningDate,
                PhotoUrl = await _fileStorageService.SaveImageAsync(request.PhotoBase64, "doctor-photos", cancellationToken)
            };

            await _doctorRepository.AddAsync(doctor);
            await _unitOfWork.SaveChangesAsync();

            return new CreateDoctorResponse
            {
                DoctorId = doctor.DoctorId,
                UserId = account.UserId,
                Username = account.Username,
                TemporaryPassword = account.TemporaryPassword
            };
        }

        private static (string? FirstName, string? LastName) SplitName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return (null, null);

            var parts = fullName.Trim().Split(' ', 2);
            return parts.Length == 2 ? (parts[0], parts[1]) : (parts[0], null);
        }
    }
}
