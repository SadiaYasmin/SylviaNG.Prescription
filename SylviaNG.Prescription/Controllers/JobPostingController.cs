using MediatR;
using Microsoft.AspNetCore.Mvc;
using SylviaNG.Prescription.Application.Features.JobPostings.Commands.JobPostingCreate;
using SylviaNG.Prescription.Application.Features.JobPostings.Commands.JobPostingDelete;
using SylviaNG.Prescription.Application.Features.JobPostings.Commands.JobPostingUpdate;
using SylviaNG.Prescription.Application.Features.JobPostings.Models;
using SylviaNG.Prescription.Application.Features.JobPostings.Queries.JobPostingGetAll;
using SylviaNG.Prescription.Application.Features.JobPostings.Queries.JobPostingGetAllPaged;
using SylviaNG.Prescription.Application.Features.JobPostings.Queries.JobPostingGetById;
using SylviaNG.Prescription.SharedKernel.Pagination;

namespace SylviaNG.Prescription.Controllers
{
    [ApiController]
    [Route("prescription/job-posting")]
    public class JobPostingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public JobPostingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all job postings.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<JobPostingResponse>>> GetAll()
        {
            var result = await _mediator.Send(new JobPostingGetAllQuery());
            return Ok(result);
        }

        /// <summary>
        /// Get a job posting by ID.
        /// </summary>
        [HttpGet("{jobPostingId}")]
        public async Task<ActionResult<JobPostingResponse>> GetById(long jobPostingId)
        {
            var result = await _mediator.Send(new JobPostingGetByIdQuery(jobPostingId));
            return Ok(result);
        }

        /// <summary>
        /// Get paginated job postings with search and sort.
        /// </summary>
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<JobPostingResponse>>> GetPaged([FromQuery] PagedRequest request)
        {
            var result = await _mediator.Send(new JobPostingGetAllPagedQuery(request));
            return Ok(result);
        }

        /// <summary>
        /// Create a new job posting.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<long>> Create([FromBody] JobPostingCreateRequest request)
        {
            var id = await _mediator.Send(new JobPostingCreateCommand(request));
            return Ok(id);
        }

        /// <summary>
        /// Update an existing job posting.
        /// </summary>
        [HttpPut("{jobPostingId}")]
        public async Task<ActionResult> Update(long jobPostingId, [FromBody] JobPostingUpdateRequest request)
        {
            await _mediator.Send(new JobPostingUpdateCommand(jobPostingId, request));
            return Ok();
        }

        /// <summary>
        /// Delete a job posting.
        /// </summary>
        [HttpDelete("{jobPostingId}")]
        public async Task<ActionResult> Delete(long jobPostingId)
        {
            await _mediator.Send(new JobPostingDeleteCommand(jobPostingId));
            return Ok();
        }
    }
}
