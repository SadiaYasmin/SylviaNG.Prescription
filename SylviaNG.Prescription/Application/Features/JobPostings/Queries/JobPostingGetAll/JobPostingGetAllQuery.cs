using MediatR;
using SylviaNG.Prescription.Application.Features.JobPostings.Models;

namespace SylviaNG.Prescription.Application.Features.JobPostings.Queries.JobPostingGetAll
{
    public class JobPostingGetAllQuery : IRequest<List<JobPostingResponse>>
    {
    }
}
