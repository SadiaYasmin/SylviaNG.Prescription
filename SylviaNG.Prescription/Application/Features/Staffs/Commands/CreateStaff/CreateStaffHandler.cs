using MediatR;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Auth.Models;
using SylviaNG.Prescription.Application.Features.Staffs.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Interfaces.Services;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Staffs.Commands.CreateStaff
{
    public class CreateStaffHandler : IRequestHandler<CreateStaffCommand, CreateStaffResponse>
    {
        private readonly IAuthService _authService;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateStaffHandler(
            IAuthService authService,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            IUnitOfWork unitOfWork)
        {
            _authService = authService;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<CreateStaffResponse> Handle(CreateStaffCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var doctorIds = request.AssignedDoctorIds.Distinct().ToList();

            foreach (var doctorId in doctorIds)
            {
                if (await _doctorRepository.GetByIdAsync(doctorId) is null)
                    throw new NotFoundException("Doctor", doctorId);
            }

            var (firstName, lastName) = SplitName(request.FullName);
            var account = await _authService.CreateUserAccountAsync(new CreateUserAccountRequest
            {
                Username = request.Username,
                Email = request.Email,
                FirstName = firstName,
                LastName = lastName,
                Role = UserRoleEnum.Staff
            });

            var staff = new Staff
            {
                UserId = account.UserId,
                FullName = request.FullName,
                Phone = request.Phone,
                Department = request.Department
            };

            await _staffRepository.AddAsync(staff);
            await _unitOfWork.SaveChangesAsync();

            foreach (var doctorId in doctorIds)
            {
                _unitOfWork.Context.StaffDoctors.Add(new StaffDoctor { StaffId = staff.StaffId, DoctorId = doctorId });
            }
            await _unitOfWork.SaveChangesAsync();

            return new CreateStaffResponse
            {
                StaffId = staff.StaffId,
                UserId = account.UserId,
                Username = account.Username,
                TemporaryPassword = account.TemporaryPassword
            };
        }

        private static (string? FirstName, string? LastName) SplitName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return (null, null);

            var parts = fullName.Trim().Split(' ', 2);
            return parts.Length == 2 ? (parts[0], parts[1]) : (parts[0], null);
        }
    }
}
