using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Features.Consultations.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Consultations.Queries.GetMyAssignedDoctors
{
    /// <summary>
    /// The doctor picker for the create-consultation flow: every doctor assigned to the
    /// calling staff member via Epic J's StaffDoctor join.
    /// </summary>
    public class GetMyAssignedDoctorsHandler : IRequestHandler<GetMyAssignedDoctorsQuery, List<AssignedDoctorSummaryResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GetMyAssignedDoctorsHandler(
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

        public async Task<List<AssignedDoctorSummaryResponse>> Handle(GetMyAssignedDoctorsQuery query, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                query.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var staffId = caller.StaffId!.Value;

            // Joined against _unitOfWork.Context.Doctors (not _doctorRepository.Query()) so
            // both sides of the join are DbSets on the SAME DbContext — mixing a DbSet with
            // a repository's separately-backed IQueryable here fails to translate (confirmed:
            // it throws at runtime, EF/the InMemory provider can't compose across two
            // unrelated query sources), same reasoning GetAssignedStaffHandler already
            // follows for its StaffDoctors/Staff/Users join.
            var doctors = await _unitOfWork.Context.StaffDoctors
                .Where(sd => sd.StaffId == staffId)
                .Join(_unitOfWork.Context.Doctors, sd => sd.DoctorId, d => d.DoctorId,
                    (sd, d) => new AssignedDoctorSummaryResponse { DoctorId = d.DoctorId, FullName = d.FullName })
                .OrderBy(d => d.FullName)
                .ToListAsync(cancellationToken);

            return doctors;
        }
    }
}
