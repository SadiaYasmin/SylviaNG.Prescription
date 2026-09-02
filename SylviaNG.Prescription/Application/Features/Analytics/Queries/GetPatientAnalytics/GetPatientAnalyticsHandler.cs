using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Features.Analytics.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetPatientAnalytics
{
    /// <summary>
    /// US-075. New = registered inside the caller-selected range, regardless of whether
    /// they've been consulted yet. Returning = registered BEFORE the range started AND has
    /// at least one Completed consultation inside it. Both are counted as distinct patients
    /// (a patient with several Completed consultations in-range still contributes exactly 1
    /// to Returning) — mirrored exactly by <c>GetPatientListHandler</c>'s NewOnly/ReturningOnly
    /// drill-down filters.
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
            var from = DateTime.SpecifyKind(query.From, DateTimeKind.Utc);
            var to = DateTime.SpecifyKind(query.To, DateTimeKind.Utc);

            var patients = await _patientRepository.Query().ToListAsync(cancellationToken);
            var allConsultations = await _consultationRepository.Query().ToListAsync(cancellationToken);
            var allFinalized = await _prescriptionRepository.Query()
                .Where(p => p.Status == PrescriptionStatusEnum.Finalized)
                .ToListAsync(cancellationToken);

            var consultationsInRange = allConsultations.Where(c => c.CheckInAt >= from && c.CheckInAt < to).ToList();
            var finalized = allFinalized.Where(p => p.FinalizedAt >= from && p.FinalizedAt < to).ToList();

            var seenPatientIdsInRange = consultationsInRange.Select(c => c.PatientId).Distinct().ToList();

            var newPatients = patients.Count(p => p.RegisteredAt >= from && p.RegisteredAt < to);

            var patientsById = patients.ToDictionary(p => p.PatientId);
            var completedPatientIdsInRange = consultationsInRange
                .Where(c => c.Status == ConsultationStatusEnum.Completed)
                .Select(c => c.PatientId)
                .Distinct()
                .ToList();
            var returningPatients = completedPatientIdsInRange.Count(id =>
                patientsById.TryGetValue(id, out var patient) && patient.RegisteredAt < from);

            var (trendStart, trendEnd) = (from, to);
            var newRegistrationTrend = AnalyticsDateBucketing.BuildTrendZeroFilled(
                patients, p => (DateTime?)p.RegisteredAt, query.Granularity, trendStart, trendEnd);

            var averageVisitsPerPatient = AnalyticsMath.SafeDivide(consultationsInRange.Count, seenPatientIdsInRange.Count, 0);

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
