using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.JobPostings.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Interfaces.Services;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.SharedKernel.Pagination;

namespace SylviaNG.Prescription.Application.Services
{
    public class JobApplicationService : IJobApplicationService
    {
        private readonly IJobApplicationRepository _jobApplicationRepository;
        private readonly IJobPostingRepository _jobPostingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public JobApplicationService(
            IJobApplicationRepository jobApplicationRepository,
            IJobPostingRepository jobPostingRepository,
            IUnitOfWork unitOfWork)
        {
            _jobApplicationRepository = jobApplicationRepository;
            _jobPostingRepository = jobPostingRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<long> CreateAsync(JobApplicationCreateRequest request)
        {
            var jobPosting = await _jobPostingRepository.GetByIdAsync(request.JobPostingId)
                ?? throw new NotFoundException("JobPosting", request.JobPostingId);

            if (!string.IsNullOrEmpty(request.CandidateEmail))
            {
                var exists = await _jobApplicationRepository.GetByEmailAndJobPostingIdAsync(request.CandidateEmail, request.JobPostingId);
                if (exists != null)
                    throw new DuplicateException("JobApplication", "CandidateEmail", request.CandidateEmail);
            }

            var entity = request.ToEntity();
            entity.ApplicationStatus = ApplicationStatusEnum.Applied;
            entity.AppliedDate = DateTime.UtcNow;

            await _jobApplicationRepository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return entity.JobApplicationId;
        }

        public async Task UpdateAsync(long jobApplicationId, JobApplicationUpdateRequest request)
        {
            var entity = await _jobApplicationRepository.GetByIdAsync(jobApplicationId)
                ?? throw new NotFoundException("JobApplication", jobApplicationId);

            entity.ApplyUpdate(request);
            _jobApplicationRepository.Update(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(long jobApplicationId)
        {
            var entity = await _jobApplicationRepository.GetByIdAsync(jobApplicationId)
                ?? throw new NotFoundException("JobApplication", jobApplicationId);

            _jobApplicationRepository.Delete(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<JobApplicationResponse> GetByIdAsync(long jobApplicationId)
        {
            var entity = await _jobApplicationRepository.GetByIdWithIncludeAsync(
                a => a.JobApplicationId == jobApplicationId,
                a => a.JobPosting)
                ?? throw new NotFoundException("JobApplication", jobApplicationId);

            return entity.ToResponse();
        }

        public async Task<PagedResult<JobApplicationResponse>> GetPaginatedByJobPostingAsync(long jobPostingId, PagedRequest request)
        {
            var pagedResult = await _jobApplicationRepository.GetPaginatedByJobPostingAsync(jobPostingId, request);

            return new PagedResult<JobApplicationResponse>
            {
                Data = pagedResult.Data.Select(e => e.ToResponse()).ToList(),
                TotalCount = pagedResult.TotalCount,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize
            };
        }
    }
}
