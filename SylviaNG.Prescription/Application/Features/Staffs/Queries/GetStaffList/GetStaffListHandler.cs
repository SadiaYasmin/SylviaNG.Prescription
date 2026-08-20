using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Features.Staffs.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Staffs.Queries.GetStaffList
{
    public class GetStaffListHandler : IRequestHandler<GetStaffListQuery, StaffListResponse>
    {
        private readonly IStaffRepository _staffRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GetStaffListHandler(IStaffRepository staffRepository, IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _staffRepository = staffRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<StaffListResponse> Handle(GetStaffListQuery query, CancellationToken cancellationToken)
        {
            var request = query.Request;
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

            var joined =
                from s in _staffRepository.Query()
                join u in _userRepository.Query() on s.UserId equals u.UserId
                select new { s, u };

            // Staff has no own Department column (a staff member can support doctors from more
            // than one department — see Staff.cs) — both the search box and the Department
            // filter match against assigned doctors' Department instead, via a StaffId subquery.
            var staffDepartments = _unitOfWork.Context.StaffDoctors
                .Join(_unitOfWork.Context.Doctors, sd => sd.DoctorId, d => d.DoctorId, (sd, d) => new { sd.StaffId, d.Department })
                .Where(x => x.Department != null);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                var staffIdsByDeptTerm = staffDepartments.Where(x => x.Department!.ToLower().Contains(term)).Select(x => x.StaffId);
                joined = joined.Where(x =>
                    x.s.FullName.ToLower().Contains(term) ||
                    x.u.Username.ToLower().Contains(term) ||
                    staffIdsByDeptTerm.Contains(x.s.StaffId));
            }

            if (!string.IsNullOrWhiteSpace(request.Department))
            {
                var staffIdsInDept = staffDepartments.Where(x => x.Department == request.Department).Select(x => x.StaffId);
                joined = joined.Where(x => staffIdsInDept.Contains(x.s.StaffId));
            }

            if (request.IsActive.HasValue)
                joined = joined.Where(x => x.u.IsActive == request.IsActive.Value);

            var totalCount = await joined.CountAsync(cancellationToken);

            var pageItems = await joined
                .OrderByDescending(x => x.s.StaffId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var staffIds = pageItems.Select(x => x.s.StaffId).ToList();

            var assignments = await _unitOfWork.Context.StaffDoctors
                .Where(sd => staffIds.Contains(sd.StaffId))
                .Join(_unitOfWork.Context.Doctors, sd => sd.DoctorId, d => d.DoctorId,
                    (sd, d) => new { sd.StaffId, Doctor = new AssignedDoctorSummary { DoctorId = d.DoctorId, FullName = d.FullName, Department = d.Department } })
                .ToListAsync(cancellationToken);

            var assignedDoctorsByStaffId = assignments.ToLookup(x => x.StaffId, x => x.Doctor);

            return new StaffListResponse
            {
                Staff = pageItems
                    .Select(x => x.s.ToSummaryResponse(x.u, assignedDoctorsByStaffId[x.s.StaffId].ToList()))
                    .ToList(),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }
    }
}
