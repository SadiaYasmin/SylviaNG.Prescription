using MediatR;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctorProfile
{
    /// <summary>
    /// A doctor editing their own profile (US-061) — deliberately never accepts a target
    /// doctor id from the request; the caller is always resolved from the JWT, so this
    /// self-service action can never be used to edit another doctor's record. Mirrors
    /// UpdateDoctorHandler's field-write, but omits the IsActive/Keycloak-enable block
    /// entirely (an Admin-only concern) and never writes LicenseNumber — that's Admin-only
    /// too (edited via UpdateDoctorHandler, which owns the duplicate-license check).
    /// </summary>
    public class UpdateDoctorProfileHandler : IRequestHandler<UpdateDoctorProfileCommand, DoctorProfileResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateDoctorProfileHandler(
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

        public async Task<DoctorProfileResponse> Handle(UpdateDoctorProfileCommand command, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                command.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var doctor = await _doctorRepository.GetByIdAsync(caller.DoctorId!.Value)
                ?? throw new NotFoundException("Doctor", caller.DoctorId!.Value);
            var user = await _userRepository.GetByIdAsync(doctor.UserId)
                ?? throw new NotFoundException("User", doctor.UserId);

            var request = command.Request;

            doctor.FullName = request.FullName;
            doctor.Phone = request.Phone;
            doctor.Qualification = request.Qualification;
            doctor.Department = request.Department;

            if (!string.IsNullOrWhiteSpace(request.Email))
                user.Email = request.Email;

            _doctorRepository.Update(doctor);
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return new DoctorProfileResponse
            {
                DoctorId = doctor.DoctorId,
                FullName = doctor.FullName,
                Qualification = doctor.Qualification,
                Department = doctor.Department,
                LicenseNumber = doctor.LicenseNumber,
                Phone = doctor.Phone,
                Email = user.Email,
                PhotoUrl = doctor.PhotoUrl
            };
        }
    }
}
