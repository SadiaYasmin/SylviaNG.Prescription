using MediatR;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;

namespace SylviaNG.Prescription.Application.Features.Doctors.Queries.GetDoctorDetails
{
    public class GetDoctorDetailsHandler : IRequestHandler<GetDoctorDetailsQuery, DoctorDetailsResponse>
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUserRepository _userRepository;

        public GetDoctorDetailsHandler(IDoctorRepository doctorRepository, IUserRepository userRepository)
        {
            _doctorRepository = doctorRepository;
            _userRepository = userRepository;
        }

        public async Task<DoctorDetailsResponse> Handle(GetDoctorDetailsQuery query, CancellationToken cancellationToken)
        {
            var doctor = await _doctorRepository.GetByIdAsync(query.DoctorId)
                ?? throw new NotFoundException("Doctor", query.DoctorId);
            var user = await _userRepository.GetByIdAsync(doctor.UserId)
                ?? throw new NotFoundException("User", doctor.UserId);

            return new DoctorDetailsResponse
            {
                Profile = doctor.ToSummaryResponse(user),
                // Consultation/Prescription don't exist yet (Epic C/D) — this stays a
                // real, stably-shaped zero/empty result until those land, per the
                // approved plan, rather than a fabricated placeholder.
                Performance = new DoctorPerformanceStats()
            };
        }
    }
}
