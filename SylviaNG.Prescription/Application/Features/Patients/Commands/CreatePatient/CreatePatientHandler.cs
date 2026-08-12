using MediatR;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Patients.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Patients.Commands.CreatePatient
{
    public class CreatePatientHandler : IRequestHandler<CreatePatientCommand, PatientSummaryResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreatePatientHandler(
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IPatientRepository patientRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _patientRepository = patientRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PatientSummaryResponse> Handle(CreatePatientCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByKeycloakIdAsync(command.KeycloakId)
                ?? throw new NotFoundException("User", command.KeycloakId);
            var staff = await _staffRepository.GetByUserIdAsync(user.UserId)
                ?? throw new NotFoundException("Staff", user.UserId);

            var request = command.Request;
            var patient = new Patient
            {
                Name = request.Name,
                Phone = request.Phone,
                DateOfBirth = request.DateOfBirth,
                Age = request.Age,
                Gender = request.Gender,
                Address = request.Address,
                BloodGroup = request.BloodGroup,
                AllergyPresetId = request.AllergyPresetId,
                AllergyOtherText = request.AllergyOtherText,
                SavedHistory = request.SavedHistory,
                // Ownership fields are server-controlled — never taken from the request body.
                RegisteredByStaffId = staff.StaffId,
                RegisteredAt = DateTime.UtcNow
            };

            await _patientRepository.AddAsync(patient);
            await _unitOfWork.SaveChangesAsync();

            return patient.ToSummaryResponse(staff.FullName);
        }
    }
}
