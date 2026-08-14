using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Queries.GetPatientPrescriptionHistory
{
    public class GetPatientPrescriptionHistoryHandler : IRequestHandler<GetPatientPrescriptionHistoryQuery, PrescriptionListResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GetPatientPrescriptionHistoryHandler(
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IPrescriptionRepository prescriptionRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _prescriptionRepository = prescriptionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PrescriptionListResponse> Handle(GetPatientPrescriptionHistoryQuery query, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                query.KeycloakId, _userRepository, _staffRepository, _doctorRepository);

            var patient = await _patientRepository.GetByIdAsync(query.PatientId)
                ?? throw new NotFoundException("Patient", query.PatientId);

            var scoped = await PrescriptionVisibilityScope.ApplyAsync(
                _prescriptionRepository.Query().Where(p => p.PatientId == query.PatientId),
                _unitOfWork.Context, caller, ownOnly: false, cancellationToken);

            var items = await scoped.OrderByDescending(p => p.FinalizedAt ?? p.SavedAt).ToListAsync(cancellationToken);
            var doctorIds = items.Select(p => p.DoctorId).Distinct().ToList();
            var doctors = (await _doctorRepository.Query().Where(d => doctorIds.Contains(d.DoctorId)).ToListAsync(cancellationToken))
                .ToDictionary(d => d.DoctorId, d => d.FullName);

            return new PrescriptionListResponse
            {
                Prescriptions = items
                    .Select(p => p.ToListItemResponse(patient.Name, doctors.GetValueOrDefault(p.DoctorId, string.Empty)))
                    .ToList()
            };
        }
    }
}
