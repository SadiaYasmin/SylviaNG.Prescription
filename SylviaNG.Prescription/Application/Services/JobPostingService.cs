using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.JobPostings.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Interfaces.Services;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.SharedKernel.Pagination;

namespace SylviaNG.Prescription.Application.Services
{
    public class JobPostingService : IJobPostingService
    {
        private readonly IJobPostingRepository _jobPostingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public JobPostingService(
            IJobPostingRepository jobPostingRepository,
            IUnitOfWork _unitOfWork)
        {
            _jobPostingRepository = jobPostingRepository;
            this._unitOfWork = _unitOfWork;
        }

        public async Task<long> CreateAsync(JobPostingCreateRequest request)
        {
            var exists = await _jobPostingRepository.ExistsByTitleAndSiteIdAsync(request.Title, request.SiteId);
            if (exists)
                throw new DuplicateException("JobPosting", "Title", request.Title);

            var entity = request.ToEntity();
            await _jobPostingRepository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return entity.JobPostingId;
        }

        public async Task UpdateAsync(long jobPostingId, JobPostingUpdateRequest request)
        {
            var entity = await _jobPostingRepository.GetByIdAsync(jobPostingId)
                ?? throw new NotFoundException("JobPosting", jobPostingId);

            entity.ApplyUpdate(request);
            _jobPostingRepository.Update(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(long jobPostingId)
        {
            var entity = await _jobPostingRepository.GetByIdAsync(jobPostingId)
                ?? throw new NotFoundException("JobPosting", jobPostingId);

            _jobPostingRepository.Delete(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<JobPostingResponse> GetByIdAsync(long jobPostingId)
        {
            var entity = await _jobPostingRepository.GetByIdWithIncludeAsync(
                j => j.JobPostingId == jobPostingId,
                j => j.Applications)
                ?? throw new NotFoundException("JobPosting", jobPostingId);

            return entity.ToResponse();
        }

        public async Task<List<JobPostingResponse>> GetAllAsync()
        {
            var entities = await _jobPostingRepository.GetAllWithIncludeAsync(j => j.Applications);
            return entities.Select(e => e.ToResponse()).ToList();
        }

        public async Task<PagedResult<JobPostingResponse>> GetPaginatedAsync(PagedRequest request)
        {
            var pagedResult = await _jobPostingRepository.GetPaginatedAsync(request);

            return new PagedResult<JobPostingResponse>
            {
                Data = pagedResult.Data.Select(e => e.ToResponse()).ToList(),
                TotalCount = pagedResult.TotalCount,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<List<JobPostingLookupResponse>> GetActiveBySiteIdAsync(long siteId)
        {
            var entities = await _jobPostingRepository.GetActiveBySiteIdAsync(siteId);
            return entities.Select(e => e.ToLookupResponse()).ToList();
        }
    }
}
