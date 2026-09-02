using MediatR;
using SylviaNG.Prescription.Application.Features.Analytics.Models;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetMedicineAnalytics
{
    public class GetMedicineAnalyticsQuery : IRequest<MedicineAnalyticsResponse>
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int TopN { get; set; }
        public int RareThreshold { get; set; }
        public int PairTopN { get; set; }

        public GetMedicineAnalyticsQuery(DateTime from, DateTime to, int topN = 10, int rareThreshold = 1, int pairTopN = 10)
        {
            From = from;
            To = to;
            TopN = topN;
            RareThreshold = rareThreshold;
            PairTopN = pairTopN;
        }
    }
}
