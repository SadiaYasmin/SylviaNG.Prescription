using SylviaNG.Prescription.Application.Features.JobPostings.Models;
using SylviaNG.Prescription.SharedKernel.Pagination;

namespace SylviaNG.Prescription.Application.Interfaces.Services
{
    public interface IJobPostingService
    {
        Task<long> CreateAsync(JobPostingCreateRequest request);
        Task UpdateAsync(long jobPostingId, JobPostingUpdateRequest request);
        Task DeleteAsync(long jobPostingId);
        Task<JobPostingResponse> GetByIdAsync(long jobPostingId);
        Task<List<JobPostingResponse>> GetAllAsync();
        Task<PagedResult<JobPostingResponse>> GetPaginatedAsync(PagedRequest request);
        Task<List<JobPostingLookupResponse>> GetActiveBySiteIdAsync(long siteId);
    }
}
