using MediatR;
using SylviaNG.Prescription.Application.Features.JobPostings.Models;
using SylviaNG.Prescription.Application.Interfaces.Services;

namespace SylviaNG.Prescription.Application.Features.JobPostings.Queries.JobPostingGetAll
{
    public class JobPostingGetAllHandler : IRequestHandler<JobPostingGetAllQuery, List<JobPostingResponse>>
    {
        private readonly IJobPostingService _jobPostingService;

        public JobPostingGetAllHandler(IJobPostingService jobPostingService)
        {
            _jobPostingService = jobPostingService;
        }

        public async Task<List<JobPostingResponse>> Handle(JobPostingGetAllQuery query, CancellationToken cancellationToken)
        {
            return await _jobPostingService.GetAllAsync();
        }
    }
}
