using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Staffs.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Staffs.Queries.GetAssignedStaff
{
    public class GetAssignedStaffHandler : IRequestHandler<GetAssignedStaffQuery, StaffListResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GetAssignedStaffHandler(IUserRepository userRepository, IDoctorRepository doctorRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _doctorRepository = doctorRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<StaffListResponse> Handle(GetAssignedStaffQuery query, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByKeycloakIdAsync(query.KeycloakId)
                ?? throw new NotFoundException("User", query.KeycloakId);
            var doctor = await _doctorRepository.GetByUserIdAsync(user.UserId)
                ?? throw new NotFoundException("Doctor", user.UserId);

            var joined =
                from sd in _unitOfWork.Context.StaffDoctors
                where sd.DoctorId == doctor.DoctorId
                join s in _unitOfWork.Context.Staff on sd.StaffId equals s.StaffId
                join u in _unitOfWork.Context.Users on s.UserId equals u.UserId
                select new { s, u };

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var term = query.SearchTerm.Trim().ToLower();
                joined = joined.Where(x => x.s.FullName.ToLower().Contains(term));
            }

            // Matches the main Staff list's convention (newest-first by StaffId), so a doctor's
            // "assigned to me" view isn't ordered differently from the admin Staff list.
            var items = await joined.OrderByDescending(x => x.s.StaffId).ToListAsync(cancellationToken);
            var staffIds = items.Select(x => x.s.StaffId).ToList();

            var assignments = await _unitOfWork.Context.StaffDoctors
                .Where(sd => staffIds.Contains(sd.StaffId))
                .Join(_unitOfWork.Context.Doctors, sd => sd.DoctorId, d => d.DoctorId,
                    (sd, d) => new { sd.StaffId, Doctor = new AssignedDoctorSummary { DoctorId = d.DoctorId, FullName = d.FullName, Department = d.Department } })
                .ToListAsync(cancellationToken);
            var assignedDoctorsByStaffId = assignments.ToLookup(x => x.StaffId, x => x.Doctor);

            var staffResponses = items
                .Select(x => x.s.ToSummaryResponse(x.u, assignedDoctorsByStaffId[x.s.StaffId].ToList()))
                .ToList();

            return new StaffListResponse
            {
                Staff = staffResponses,
                TotalCount = staffResponses.Count,
                PageNumber = 1,
                PageSize = staffResponses.Count
            };
        }
    }
}
