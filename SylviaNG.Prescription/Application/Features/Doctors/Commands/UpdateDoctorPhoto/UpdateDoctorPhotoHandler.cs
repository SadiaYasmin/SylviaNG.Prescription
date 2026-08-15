using MediatR;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctorPhoto
{
    public class UpdateDoctorPhotoHandler : IRequestHandler<UpdateDoctorPhotoCommand, DoctorProfileResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateDoctorPhotoHandler(
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

        public async Task<DoctorProfileResponse> Handle(UpdateDoctorPhotoCommand command, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                command.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var doctor = await _doctorRepository.GetByIdAsync(caller.DoctorId!.Value)
                ?? throw new NotFoundException("Doctor", caller.DoctorId!.Value);
            var user = await _userRepository.GetByIdAsync(doctor.UserId)
                ?? throw new NotFoundException("User", doctor.UserId);

            doctor.PhotoBase64 = command.Request.PhotoBase64;

            _doctorRepository.Update(doctor);
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
                PhotoBase64 = doctor.PhotoBase64
            };
        }
    }
}
