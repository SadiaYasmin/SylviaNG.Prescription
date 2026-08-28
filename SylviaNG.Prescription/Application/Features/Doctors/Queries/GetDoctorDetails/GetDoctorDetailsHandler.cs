using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Analytics;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Doctors.Queries.GetDoctorDetails
{
    /// <summary>
    /// US-055's per-doctor performance drill-down, filled in for real now that
    /// Consultation/Prescription exist (Epic M/US-073 payoff — this handler previously
    /// returned a stably-shaped zero <see cref="DoctorPerformanceStats"/> by design). Reuses
    /// <see cref="Analytics.MedicinePrescribingAggregator"/>/<see cref="Analytics.AnalyticsDateBucketing"/>/
    /// <see cref="Analytics.AnalyticsMath"/> — the same aggregation building blocks every
    /// other Analytics handler shares.
    /// </summary>
    public class GetDoctorDetailsHandler : IRequestHandler<GetDoctorDetailsQuery, DoctorDetailsResponse>
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConsultationRepository _consultationRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IPatientRepository _patientRepository;

        public GetDoctorDetailsHandler(
            IDoctorRepository doctorRepository,
            IUserRepository userRepository,
            IConsultationRepository consultationRepository,
            IPrescriptionRepository prescriptionRepository,
            IPatientRepository patientRepository)
        {
            _doctorRepository = doctorRepository;
            _userRepository = userRepository;
            _consultationRepository = consultationRepository;
            _prescriptionRepository = prescriptionRepository;
            _patientRepository = patientRepository;
        }

        public async Task<DoctorDetailsResponse> Handle(GetDoctorDetailsQuery query, CancellationToken cancellationToken)
        {
            var doctor = await _doctorRepository.GetByIdAsync(query.DoctorId)
                ?? throw new NotFoundException("Doctor", query.DoctorId);
            var user = await _userRepository.GetByIdAsync(doctor.UserId)
                ?? throw new NotFoundException("User", doctor.UserId);

            var consultations = await _consultationRepository.Query()
                .Where(c => c.DoctorId == doctor.DoctorId)
                .ToListAsync(cancellationToken);

            var finalized = await _prescriptionRepository.Query()
                .Where(p => p.DoctorId == doctor.DoctorId && p.Status == PrescriptionStatusEnum.Finalized)
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;
            var totalMedicinesPrescribed = finalized.Sum(p => p.GetMedicines().Count);
            var completedConsultations = consultations.Count(c => c.Status == ConsultationStatusEnum.Completed);

            var aggregation = MedicinePrescribingAggregator.Aggregate(finalized);
            var allMedicines = aggregation.CountsByKey
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => new DoctorTopMedicine { Name = aggregation.LabelByKey[kvp.Key], Count = kvp.Value })
                .ToList();
            var topMedicines = allMedicines.Take(5).ToList();

            var recentPrescriptionRecords = finalized
                .OrderByDescending(p => p.FinalizedAt)
                .Take(5)
                .ToList();
            var patientIds = recentPrescriptionRecords.Select(p => p.PatientId).Distinct().ToList();
            var patientNames = await _patientRepository.Query()
                .Where(p => patientIds.Contains(p.PatientId))
                .ToDictionaryAsync(p => p.PatientId, p => p.Name, cancellationToken);

            var recentPrescriptions = recentPrescriptionRecords
                .Select(p => new DoctorRecentPrescription
                {
                    PrescriptionId = p.DisplayCode,
                    PatientName = patientNames.GetValueOrDefault(p.PatientId, string.Empty),
                    Diagnosis = p.GetDiagnoses().FirstOrDefault()?.Text,
                    Date = p.FinalizedAt ?? p.SavedAt ?? now
                })
                .ToList();

            var (trendStart, trendEnd) = AnalyticsDateBucketing.GetDefaultRange(query.ActivityGranularity, now);
            var activityTrend = AnalyticsDateBucketing
                .BuildTrendZeroFilled(finalized, p => p.FinalizedAt, query.ActivityGranularity, trendStart, trendEnd)
                .Select(point => new DoctorActivityTrendPoint { Period = AnalyticsDateBucketing.ParseBucketKey(point.BucketKey), Count = point.Count })
                .ToList();

            var countsByBdtHour = consultations
                .GroupBy(c => (c.CheckInAt.Hour + AnalyticsDateBucketing.BangladeshUtcOffsetHours) % 24)
                .ToDictionary(g => g.Key, g => g.Count());

            var busiestHours = Enumerable.Range(0, 24)
                .Select(hour => new HourBucket { Hour = hour, Count = countsByBdtHour.GetValueOrDefault(hour) })
                .ToList();

            return new DoctorDetailsResponse
            {
                Profile = doctor.ToSummaryResponse(user),
                Performance = new DoctorPerformanceStats
                {
                    TotalPatientsConsulted = consultations.Select(c => c.PatientId).Distinct().Count(),
                    TotalPrescriptions = finalized.Count,
                    TodayPrescriptions = finalized.Count(p => p.FinalizedAt?.Date == now.Date),
                    ThisMonthPrescriptions = finalized.Count(p => p.FinalizedAt?.Year == now.Year && p.FinalizedAt?.Month == now.Month),
                    AvgPrescriptionsPerConsultation = AnalyticsMath.SafeDivide(finalized.Count, completedConsultations),
                    AvgMedicinesPerPrescription = AnalyticsMath.SafeDivide(totalMedicinesPrescribed, finalized.Count),
                    TotalMedicinesPrescribed = totalMedicinesPrescribed,
                    TopMedicines = topMedicines,
                    AllMedicines = allMedicines,
                    RecentPrescriptions = recentPrescriptions,
                    ActivityTrend = activityTrend,
                    BusiestHours = busiestHours
                }
            };
        }
    }
}
