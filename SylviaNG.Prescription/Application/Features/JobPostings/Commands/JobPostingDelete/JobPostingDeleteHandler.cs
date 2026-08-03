using MediatR;
using SylviaNG.Prescription.Application.Interfaces.Services;

namespace SylviaNG.Prescription.Application.Features.JobPostings.Commands.JobPostingDelete
{
    public class JobPostingDeleteHandler : IRequestHandler<JobPostingDeleteCommand, Unit>
    {
        private readonly IJobPostingService _jobPostingService;

        public JobPostingDeleteHandler(IJobPostingService jobPostingService)
        {
            _jobPostingService = jobPostingService;
        }

        public async Task<Unit> Handle(JobPostingDeleteCommand command, CancellationToken cancellationToken)
        {
            await _jobPostingService.DeleteAsync(command.JobPostingId);
            return Unit.Value;
        }
    }
}
