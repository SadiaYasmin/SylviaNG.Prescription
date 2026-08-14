using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Queries.VerifyPrescription
{
    /// <summary>
    /// Public verification (US-035). Only ever returns a Finalized prescription — an
    /// in-progress draft's id must 404 exactly like an unknown id, never leak in-progress
    /// content or confirm the id exists at all.
    /// </summary>
    public class VerifyPrescriptionHandler : IRequestHandler<VerifyPrescriptionQuery, PrescriptionDocumentResponse>
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly IHospitalSettingsRepository _hospitalSettingsRepository;

        public VerifyPrescriptionHandler(
            IPatientRepository patientRepository,
            IDoctorRepository doctorRepository,
            IPrescriptionRepository prescriptionRepository,
            ITemplateRepository templateRepository,
            IHospitalSettingsRepository hospitalSettingsRepository)
        {
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
            _prescriptionRepository = prescriptionRepository;
            _templateRepository = templateRepository;
            _hospitalSettingsRepository = hospitalSettingsRepository;
        }

        public async Task<PrescriptionDocumentResponse> Handle(VerifyPrescriptionQuery query, CancellationToken cancellationToken)
        {
            var prescription = await _prescriptionRepository.Query()
                .FirstOrDefaultAsync(p => p.DisplayCode == query.DisplayCode && p.Status == PrescriptionStatusEnum.Finalized, cancellationToken)
                ?? throw new NotFoundException("Prescription", query.DisplayCode);

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
