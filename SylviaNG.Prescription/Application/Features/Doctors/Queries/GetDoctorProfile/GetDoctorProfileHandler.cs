using MediatR;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;

namespace SylviaNG.Prescription.Application.Features.Doctors.Queries.GetDoctorProfile
{
    public class GetDoctorProfileHandler : IRequestHandler<GetDoctorProfileQuery, DoctorProfileResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;

        public GetDoctorProfileHandler(IUserRepository userRepository, IStaffRepository staffRepository, IDoctorRepository doctorRepository)
        {
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
        }

        public async Task<DoctorProfileResponse> Handle(GetDoctorProfileQuery query, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                query.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var doctor = await _doctorRepository.GetByIdAsync(caller.DoctorId!.Value)
                ?? throw new NotFoundException("Doctor", caller.DoctorId!.Value);
            var user = await _userRepository.GetByIdAsync(doctor.UserId)
                ?? throw new NotFoundException("User", doctor.UserId);

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
