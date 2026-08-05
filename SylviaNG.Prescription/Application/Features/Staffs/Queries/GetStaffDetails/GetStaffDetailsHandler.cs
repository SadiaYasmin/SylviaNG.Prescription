using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Staffs.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Staffs.Queries.GetStaffDetails
{
    public class GetStaffDetailsHandler : IRequestHandler<GetStaffDetailsQuery, StaffDetailsResponse>
    {
        private readonly IStaffRepository _staffRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GetStaffDetailsHandler(IStaffRepository staffRepository, IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _staffRepository = staffRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<StaffDetailsResponse> Handle(GetStaffDetailsQuery query, CancellationToken cancellationToken)
        {
            var staff = await _staffRepository.GetByIdAsync(query.StaffId)
                ?? throw new NotFoundException("Staff", query.StaffId);
            var user = await _userRepository.GetByIdAsync(staff.UserId)
                ?? throw new NotFoundException("User", staff.UserId);

            var assignedDoctors = await _unitOfWork.Context.StaffDoctors
                .Where(sd => sd.StaffId == staff.StaffId)
                .Join(_unitOfWork.Context.Doctors, sd => sd.DoctorId, d => d.DoctorId,
                    (sd, d) => new AssignedDoctorSummary { DoctorId = d.DoctorId, FullName = d.FullName })
                .ToListAsync(cancellationToken);

            return new StaffDetailsResponse
            {
                Profile = staff.ToSummaryResponse(user, assignedDoctors)
            };
        }
    }
}
