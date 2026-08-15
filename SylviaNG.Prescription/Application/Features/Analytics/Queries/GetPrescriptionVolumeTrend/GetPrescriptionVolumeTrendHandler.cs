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
            var finalized = await _prescriptionRepository.Query()
                .Where(p => p.Status == PrescriptionStatusEnum.Finalized)
                .ToListAsync(cancellationToken);

            var points = AnalyticsDateBucketing.BuildTrend(finalized, p => p.FinalizedAt, query.Granularity);

            return new PrescriptionVolumeTrendResponse
            {
                Granularity = query.Granularity,
                Points = points
            };
        }
    }
}
