using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Features.Analytics.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetPrescriptionVolumeTrend
{
    public class GetPrescriptionVolumeTrendHandler : IRequestHandler<GetPrescriptionVolumeTrendQuery, PrescriptionVolumeTrendResponse>
    {
        private readonly IPrescriptionRepository _prescriptionRepository;

        public GetPrescriptionVolumeTrendHandler(IPrescriptionRepository prescriptionRepository)
        {
            _prescriptionRepository = prescriptionRepository;
        }

        public async Task<PrescriptionVolumeTrendResponse> Handle(GetPrescriptionVolumeTrendQuery query, CancellationToken cancellationToken)
        {
            var (defaultStart, defaultEnd) = AnalyticsDateBucketing.GetDefaultRange(query.Granularity, DateTime.UtcNow);

            // [FromQuery] model binding produces DateTimeKind.Unspecified — Npgsql refuses to
            // compare that against a "timestamp with time zone" column ("Cannot write DateTime
            // with Kind=Unspecified"). The frontend only ever sends plain yyyy-MM-dd dates with
            // no timezone offset of their own, so relabeling as UTC (not converting) is correct.
            var rangeStart = query.From.HasValue ? DateTime.SpecifyKind(query.From.Value, DateTimeKind.Utc) : defaultStart;
            var rangeEnd = query.To.HasValue ? DateTime.SpecifyKind(query.To.Value, DateTimeKind.Utc) : defaultEnd;

            var finalized = await _prescriptionRepository.Query()
                .Where(p => p.Status == PrescriptionStatusEnum.Finalized
                    && p.FinalizedAt != null
                    && p.FinalizedAt >= rangeStart
                    && p.FinalizedAt <= rangeEnd)
                .ToListAsync(cancellationToken);

            var points = AnalyticsDateBucketing.BuildTrendZeroFilled(finalized, p => p.FinalizedAt, query.Granularity, rangeStart, rangeEnd);

            return new PrescriptionVolumeTrendResponse
            {
                Granularity = query.Granularity,
                Points = points
            };
        }
    }
}
