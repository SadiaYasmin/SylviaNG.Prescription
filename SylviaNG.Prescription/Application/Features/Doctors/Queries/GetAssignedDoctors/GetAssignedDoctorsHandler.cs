using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Doctors.Queries.GetAssignedDoctors
{
    public class GetAssignedDoctorsHandler : IRequestHandler<GetAssignedDoctorsQuery, AssignedDoctorListResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GetAssignedDoctorsHandler(
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

        public async Task<AssignedDoctorListResponse> Handle(GetAssignedDoctorsQuery query, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                query.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var staffId = caller.StaffId!.Value;

            // StaffDoctorId descending as the "most recently assigned first" proxy — StaffDoctor's
            // own CreatedAt is never populated (same reasoning as every other list in this codebase
            // that orders by PK instead), so insertion-order-via-PK is the only reliable signal.
            // OrderBy applied last (after both joins), not first, since ordering is not guaranteed
            // to survive a subsequent Join otherwise.
            var joined =
                from sd in _unitOfWork.Context.StaffDoctors
                where sd.StaffId == staffId
                join d in _unitOfWork.Context.Doctors on sd.DoctorId equals d.DoctorId
                join u in _unitOfWork.Context.Users on d.UserId equals u.UserId
                orderby sd.StaffDoctorId descending
                select new AssignedDoctorListItem
                {
                    DoctorId = d.DoctorId,
                    FullName = d.FullName,
                    Department = d.Department,
                    Phone = d.Phone,
                    IsActive = u.IsActive
                };

            var doctors = await joined.ToListAsync(cancellationToken);

            return new AssignedDoctorListResponse { Doctors = doctors };
        }
    }
}
