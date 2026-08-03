using MediatR;
using SylviaNG.Prescription.Application.Features.JobPostings.Models;

namespace SylviaNG.Prescription.Application.Features.JobPostings.Queries.JobPostingGetById
{
    public class JobPostingGetByIdQuery : IRequest<JobPostingResponse>
    {
        public long JobPostingId { get; set; }

        public JobPostingGetByIdQuery(long jobPostingId)
        {
            JobPostingId = jobPostingId;
        }
    }
}
