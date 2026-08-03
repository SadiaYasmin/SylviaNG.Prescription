using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.SharedKernel.Pagination;

namespace SylviaNG.Prescription.Application.Interfaces.Repositories
{
    public interface IJobApplicationRepository : IRepository<JobApplication>
    {
        Task<JobApplication?> GetByEmailAndJobPostingIdAsync(string email, long jobPostingId);
        Task<PagedResult<JobApplication>> GetPaginatedByJobPostingAsync(long jobPostingId, PagedRequest request);
    }
}
