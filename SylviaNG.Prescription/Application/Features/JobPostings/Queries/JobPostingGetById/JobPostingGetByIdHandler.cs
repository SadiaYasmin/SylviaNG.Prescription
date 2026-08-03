using MediatR;
using SylviaNG.Prescription.Application.Features.JobPostings.Models;
using SylviaNG.Prescription.Application.Interfaces.Services;

namespace SylviaNG.Prescription.Application.Features.JobPostings.Queries.JobPostingGetById
{
    public class JobPostingGetByIdHandler : IRequestHandler<JobPostingGetByIdQuery, JobPostingResponse>
    {
        private readonly IJobPostingService _jobPostingService;

        public JobPostingGetByIdHandler(IJobPostingService jobPostingService)
        {
            _jobPostingService = jobPostingService;
        }

        public async Task<JobPostingResponse> Handle(JobPostingGetByIdQuery query, CancellationToken cancellationToken)
        {
            return await _jobPostingService.GetByIdAsync(query.JobPostingId);
        }
    }
}
