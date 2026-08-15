using MediatR;
using SylviaNG.Prescription.Application.Features.Analytics.Models;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetMedicineAnalytics
{
    public class GetMedicineAnalyticsQuery : IRequest<MedicineAnalyticsResponse>
    {
        public int TopN { get; set; }
        public int RareThreshold { get; set; }
        public int PairTopN { get; set; }

        public GetMedicineAnalyticsQuery(int topN = 10, int rareThreshold = 1, int pairTopN = 10)
        {
            TopN = topN;
            RareThreshold = rareThreshold;
            PairTopN = pairTopN;
        }
    }
}
