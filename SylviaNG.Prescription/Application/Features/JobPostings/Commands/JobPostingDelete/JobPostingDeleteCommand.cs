using MediatR;

namespace SylviaNG.Prescription.Application.Features.JobPostings.Commands.JobPostingDelete
{
    public class JobPostingDeleteCommand : IRequest<Unit>
    {
        public long JobPostingId { get; set; }

        public JobPostingDeleteCommand(long jobPostingId)
        {
            JobPostingId = jobPostingId;
        }
    }
}
