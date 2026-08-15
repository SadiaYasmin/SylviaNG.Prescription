using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Features.Analytics.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetPatientAnalytics
{
    /// <summary>
    /// US-075. "New vs returning" is a whole-population classification over all-time
    /// consultation counts (<c>&lt;=1</c> visit ever = new, <c>&gt;1</c> = returning) — not a
    /// per-visit flag — matching the reference prototype exactly.
    /// </summary>
    public class GetPatientAnalyticsHandler : IRequestHandler<GetPatientAnalyticsQuery, PatientAnalyticsResponse>
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IConsultationRepository _consultationRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;

        public GetPatientAnalyticsHandler(
            IPatientRepository patientRepository,
            IConsultationRepository consultationRepository,
            IPrescriptionRepository prescriptionRepository)
        {
            _patientRepository = patientRepository;
            _consultationRepository = consultationRepository;
            _prescriptionRepository = prescriptionRepository;
        }

        public async Task<PatientAnalyticsResponse> Handle(GetPatientAnalyticsQuery query, CancellationToken cancellationToken)
        {
            var patients = await _patientRepository.Query().ToListAsync(cancellationToken);
            var consultations = await _consultationRepository.Query().ToListAsync(cancellationToken);
            var finalized = await _prescriptionRepository.Query()
                .Where(p => p.Status == PrescriptionStatusEnum.Finalized)
                .ToListAsync(cancellationToken);

            var visitCountsByPatient = consultations.GroupBy(c => c.PatientId).ToDictionary(g => g.Key, g => g.Count());

            var newPatients = 0;
            var returningPatients = 0;
            foreach (var patient in patients)
            {
                var visits = visitCountsByPatient.GetValueOrDefault(patient.PatientId);
                if (visits <= 1)
                {
                    newPatients++;
                }
                else
                {
                    returningPatients++;
                }
            }

            var newRegistrationTrend = AnalyticsDateBucketing.BuildTrend(
                patients, p => (DateTime?)p.RegisteredAt, AnalyticsGranularity.Day);

            var averageVisitsPerPatient = AnalyticsMath.SafeDivide(consultations.Count, patients.Count, 0);

            var topDiagnoses = AnalyticsDiagnosisAggregator.TopDiagnoses(finalized, 10);
            var chronicPatterns = AnalyticsDiagnosisAggregator.ChronicPatterns(finalized);

            var patientNames = patients.ToDictionary(p => p.PatientId, p => p.Name);
            foreach (var entry in chronicPatterns)
            {
                entry.PatientName = patientNames.GetValueOrDefault(entry.PatientId, string.Empty);
            }

            return new PatientAnalyticsResponse
            {
                NewPatients = newPatients,
                ReturningPatients = returningPatients,
                NewRegistrationTrend = newRegistrationTrend,
                AverageVisitsPerPatient = averageVisitsPerPatient,
                TopDiagnoses = topDiagnoses,
                ChronicDiagnosisPatterns = chronicPatterns
            };
        }
    }
}
