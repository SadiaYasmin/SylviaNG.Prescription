using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Features.Analytics.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetMyStaffAnalytics
{
    /// <summary>
    /// US-078. "My today's queue" needs no new backend work here — <c>GetMyQueueHandler</c>
    /// already exists and is already wired into the frontend dashboard.
    /// </summary>
    public class GetMyStaffAnalyticsHandler : IRequestHandler<GetMyStaffAnalyticsQuery, MyStaffAnalyticsResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GetMyStaffAnalyticsHandler(
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<MyStaffAnalyticsResponse> Handle(GetMyStaffAnalyticsQuery query, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                query.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var staffId = caller.StaffId!.Value;

            var patientsRegisteredByMe = await _patientRepository.Query()
                .CountAsync(p => p.RegisteredByStaffId == staffId, cancellationToken);

            // Ordered most-recently-assigned first (StaffDoctorId descending, the same
            // insertion-order-via-PK proxy GetAssignedDoctorsHandler uses — StaffDoctor's own
            // CreatedAt is never populated) so the dashboard card can show assignedDoctors[0] as
            // the "most recently assigned doctor" preview without re-sorting client-side.
            var assignedDoctorsQuery =
                from sd in _unitOfWork.Context.StaffDoctors
                where sd.StaffId == staffId
                join d in _unitOfWork.Context.Doctors on sd.DoctorId equals d.DoctorId
                orderby sd.StaffDoctorId descending
                select new AssignedDoctorEntry { DoctorId = d.DoctorId, FullName = d.FullName, Department = d.Department };
            var assignedDoctors = await assignedDoctorsQuery.ToListAsync(cancellationToken);

            return new MyStaffAnalyticsResponse
            {
                PatientsRegisteredByMe = patientsRegisteredByMe,
                AssignedDoctors = assignedDoctors
            };
        }
    }
}
