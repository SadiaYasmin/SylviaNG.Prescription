using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.SharedKernel.Pagination;

namespace SylviaNG.Prescription.Infrastructure.Repositories
{
    public class JobApplicationRepository : Repository<JobApplication>, IJobApplicationRepository
    {
        public JobApplicationRepository(ApplicationDBContext dbContext) : base(dbContext) { }

        public async Task<JobApplication?> GetByEmailAndJobPostingIdAsync(string email, long jobPostingId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(a => a.CandidateEmail == email && a.JobPostingId == jobPostingId);
        }

        public async Task<PagedResult<JobApplication>> GetPaginatedByJobPostingAsync(long jobPostingId, PagedRequest request)
        {
            var query = _dbSet
                .Include(a => a.Interviews)
                .Where(a => a.JobPostingId == jobPostingId)
                .AsQueryable();

            return await query.ToPaginatedResultAsync(request);
        }
    }
}
