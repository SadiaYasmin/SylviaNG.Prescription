using MediatR;
using Microsoft.AspNetCore.Mvc;
using SylviaNG.Prescription.Application.Features.JobPostings.Models;
using SylviaNG.Prescription.Application.Interfaces.Services;
using SylviaNG.Prescription.SharedKernel.Pagination;

namespace SylviaNG.Prescription.Controllers
{
    [ApiController]
    [Route("prescription/job-application")]
    public class JobApplicationController : ControllerBase
    {
        private readonly IJobApplicationService _jobApplicationService;

        public JobApplicationController(IJobApplicationService jobApplicationService)
        {
            _jobApplicationService = jobApplicationService;
        }

        /// <summary>
        /// Get a job application by ID.
        /// </summary>
        [HttpGet("{jobApplicationId}")]
        public async Task<ActionResult<JobApplicationResponse>> GetById(long jobApplicationId)
        {
            var result = await _jobApplicationService.GetByIdAsync(jobApplicationId);
            return Ok(result);
        }

        /// <summary>
        /// Get paginated job applications for a specific job posting.
        /// </summary>
        [HttpGet("job-posting/{jobPostingId}/paged")]
        public async Task<ActionResult<PagedResult<JobApplicationResponse>>> GetPagedByJobPosting(long jobPostingId, [FromQuery] PagedRequest request)
        {
            var result = await _jobApplicationService.GetPaginatedByJobPostingAsync(jobPostingId, request);
            return Ok(result);
        }

        /// <summary>
        /// Create a new job application.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<long>> Create([FromBody] JobApplicationCreateRequest request)
        {
            var id = await _jobApplicationService.CreateAsync(request);
            return Ok(id);
        }

        /// <summary>
        /// Update an existing job application.
        /// </summary>
        [HttpPut("{jobApplicationId}")]
        public async Task<ActionResult> Update(long jobApplicationId, [FromBody] JobApplicationUpdateRequest request)
        {
            await _jobApplicationService.UpdateAsync(jobApplicationId, request);
            return Ok();
        }

        /// <summary>
        /// Delete a job application.
        /// </summary>
        [HttpDelete("{jobApplicationId}")]
        public async Task<ActionResult> Delete(long jobApplicationId)
        {
            await _jobApplicationService.DeleteAsync(jobApplicationId);
            return Ok();
        }
    }
}
