using SylviaNG.Prescription.Application.Common.Models;

namespace SylviaNG.Prescription.Application.Interfaces.Externals
{
    public interface ICoreGrpcClient
    {
        Task<CoreBatchLookupResult> GetSitesAsync(List<long> siteIds);
    }
}
