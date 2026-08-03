using MediatR;
using SylviaNG.Prescription.Application.Features.JobPostings.Models;

namespace SylviaNG.Prescription.Application.Features.JobPostings.Commands.JobPostingCreate
{
    public class JobPostingCreateCommand : IRequest<long>
    {
        public JobPostingCreateRequest Request { get; set; }

        public JobPostingCreateCommand(JobPostingCreateRequest request)
        {
            Request = request;
        }
    }
}
