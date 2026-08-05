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

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                joined = joined.Where(x =>
                    x.s.FullName.ToLower().Contains(term) ||
                    x.u.Username.ToLower().Contains(term) ||
                    (x.s.Department != null && x.s.Department.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(request.Department))
                joined = joined.Where(x => x.s.Department == request.Department);

            if (request.IsActive.HasValue)
                joined = joined.Where(x => x.u.IsActive == request.IsActive.Value);

            var totalCount = await joined.CountAsync(cancellationToken);

            var pageItems = await joined
                .OrderBy(x => x.s.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var staffIds = pageItems.Select(x => x.s.StaffId).ToList();

            var assignments = await _unitOfWork.Context.StaffDoctors
                .Where(sd => staffIds.Contains(sd.StaffId))
                .Join(_unitOfWork.Context.Doctors, sd => sd.DoctorId, d => d.DoctorId,
                    (sd, d) => new { sd.StaffId, Doctor = new AssignedDoctorSummary { DoctorId = d.DoctorId, FullName = d.FullName } })
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
