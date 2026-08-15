using MediatR;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;

namespace SylviaNG.Prescription.Application.Features.Doctors.Queries.GetDoctorPreferences
{
    public class GetDoctorPreferencesHandler : IRequestHandler<GetDoctorPreferencesQuery, DoctorPreferencesResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;

        public GetDoctorPreferencesHandler(IUserRepository userRepository, IStaffRepository staffRepository, IDoctorRepository doctorRepository)
        {
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
        }

        public async Task<DoctorPreferencesResponse> Handle(GetDoctorPreferencesQuery query, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                query.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var doctor = await _doctorRepository.GetByIdAsync(caller.DoctorId!.Value)
                ?? throw new NotFoundException("Doctor", caller.DoctorId!.Value);

            return new DoctorPreferencesResponse
            {
                PreferredTemplateId = doctor.PreferredTemplateId,
                SignatureUrl = doctor.SignatureUrl,
                PreferredLanguage = doctor.PreferredLanguage
            };
        }
    }
}
