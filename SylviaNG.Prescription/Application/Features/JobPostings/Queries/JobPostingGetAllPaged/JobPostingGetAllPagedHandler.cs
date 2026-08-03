using MediatR;
using SylviaNG.Prescription.Application.Features.JobPostings.Models;
using SylviaNG.Prescription.Application.Interfaces.Services;
using SylviaNG.Prescription.SharedKernel.Pagination;

namespace SylviaNG.Prescription.Application.Features.JobPostings.Queries.JobPostingGetAllPaged
{
    public class JobPostingGetAllPagedHandler : IRequestHandler<JobPostingGetAllPagedQuery, PagedResult<JobPostingResponse>>
    {
        private readonly IJobPostingService _jobPostingService;

        public JobPostingGetAllPagedHandler(IJobPostingService jobPostingService)
        {
            _jobPostingService = jobPostingService;
        }

        public async Task<PagedResult<JobPostingResponse>> Handle(JobPostingGetAllPagedQuery query, CancellationToken cancellationToken)
        {
            return await _jobPostingService.GetPaginatedAsync(query.Request);
        }
    }
}
