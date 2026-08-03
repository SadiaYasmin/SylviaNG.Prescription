using MediatR;
using SylviaNG.Prescription.Application.Features.JobPostings.Models;
using SylviaNG.Prescription.SharedKernel.Pagination;

namespace SylviaNG.Prescription.Application.Features.JobPostings.Queries.JobPostingGetAllPaged
{
    public class JobPostingGetAllPagedQuery : IRequest<PagedResult<JobPostingResponse>>
    {
        public PagedRequest Request { get; set; }

        public JobPostingGetAllPagedQuery(PagedRequest request)
        {
            Request = request;
        }
    }
}
