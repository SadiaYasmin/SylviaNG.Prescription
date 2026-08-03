using SylviaNG.Prescription.Application.Features.JobPostings.Models;
using SylviaNG.Prescription.SharedKernel.Pagination;

namespace SylviaNG.Prescription.Application.Interfaces.Services
{
    public interface IJobApplicationService
    {
        Task<long> CreateAsync(JobApplicationCreateRequest request);
        Task UpdateAsync(long jobApplicationId, JobApplicationUpdateRequest request);
        Task DeleteAsync(long jobApplicationId);
        Task<JobApplicationResponse> GetByIdAsync(long jobApplicationId);
        Task<PagedResult<JobApplicationResponse>> GetPaginatedByJobPostingAsync(long jobPostingId, PagedRequest request);
    }
}
