using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.SharedKernel.Pagination;

namespace SylviaNG.Prescription.Application.Interfaces.Repositories
{
    public interface IJobPostingRepository : IRepository<JobPosting>
    {
        Task<JobPosting?> GetByTitleAndSiteIdAsync(string title, long siteId);
        Task<bool> ExistsByTitleAndSiteIdAsync(string title, long siteId, long? excludeId = null);
        Task<PagedResult<JobPosting>> GetPaginatedAsync(PagedRequest request);
        Task<List<JobPosting>> GetActiveBySiteIdAsync(long siteId);
    }
}
