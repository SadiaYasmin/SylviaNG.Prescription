using MediatR;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Queries.GetPrescriptionDetails
{
    public class GetPrescriptionDetailsHandler : IRequestHandler<GetPrescriptionDetailsQuery, PrescriptionDocumentResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly IHospitalSettingsRepository _hospitalSettingsRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GetPrescriptionDetailsHandler(
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IPrescriptionRepository prescriptionRepository,
            ITemplateRepository templateRepository,
            IHospitalSettingsRepository hospitalSettingsRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _prescriptionRepository = prescriptionRepository;
            _templateRepository = templateRepository;
            _hospitalSettingsRepository = hospitalSettingsRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PrescriptionDocumentResponse> Handle(GetPrescriptionDetailsQuery query, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                query.KeycloakId, _userRepository, _staffRepository, _doctorRepository);

            var prescription = await _prescriptionRepository.GetByIdAsync(query.PrescriptionId)
                ?? throw new NotFoundException("Prescription", query.PrescriptionId);

            // Out-of-scope reported the same way as not-found (404, not 403) — same reasoning
            // as PatientVisibilityScope/Consultation's OpenConsultation.
            var isVisible = await PrescriptionVisibilityScope.IsVisibleAsync(
                prescription, _unitOfWork.Context, caller, ownOnly: false, cancellationToken);
            if (!isVisible)
                throw new NotFoundException("Prescription", query.PrescriptionId);

            var patient = await _patientRepository.GetByIdAsync(prescription.PatientId)
                ?? throw new NotFoundException("Patient", prescription.PatientId);
            var doctor = await _doctorRepository.GetByIdAsync(prescription.DoctorId)
                ?? throw new NotFoundException("Doctor", prescription.DoctorId);
            var template = await _templateRepository.GetByIdAsync(prescription.TemplateId)
                ?? throw new NotFoundException("PrescriptionTemplate", prescription.TemplateId);
            var hospitalSettings = (await _hospitalSettingsRepository.GetAllAsync()).FirstOrDefault()
                ?? new Domain.Entities.HospitalSettings();

            return prescription.ToDocumentResponse(patient, doctor, template, hospitalSettings);
        }
    }
}
