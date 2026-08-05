using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SylviaNG.Prescription.Application.Features.Staffs.Commands.CreateStaff;
using SylviaNG.Prescription.Application.Features.Staffs.Commands.DeactivateStaff;
using SylviaNG.Prescription.Application.Features.Staffs.Commands.UpdateStaff;
using SylviaNG.Prescription.Application.Features.Staffs.Models;
using SylviaNG.Prescription.Application.Features.Staffs.Queries.GetAssignedStaff;
using SylviaNG.Prescription.Application.Features.Staffs.Queries.GetStaffDetails;
using SylviaNG.Prescription.Application.Features.Staffs.Queries.GetStaffList;

namespace SylviaNG.Prescription.Controllers
{
    /// <summary>
    /// Staff Management (Epic J / US-057–060). Admin gets full roster CRUD; a Doctor
    /// gets a separate, read-only "assigned to me" view (US-059) — no class-level
    /// [Authorize] since these two surfaces need different roles, unlike DoctorsController.
    /// </summary>
    [ApiController]
    [Route("prescription/staff")]
    public class StaffController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StaffController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<CreateStaffResponse>> Create([FromBody] CreateStaffRequest request)
        {
            var result = await _mediator.Send(new CreateStaffCommand(request));
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<StaffSummaryResponse>> Update(long id, [FromBody] UpdateStaffRequest request)
        {
            var result = await _mediator.Send(new UpdateStaffCommand(id, request));
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Deactivate(long id)
        {
            await _mediator.Send(new DeactivateStaffCommand(id));
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<StaffListResponse>> GetList([FromQuery] StaffListRequest request)
        {
            var result = await _mediator.Send(new GetStaffListQuery(request));
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<StaffDetailsResponse>> GetDetails(long id)
        {
            var result = await _mediator.Send(new GetStaffDetailsQuery(id));
            return Ok(result);
        }

        /// <summary>
        /// US-059: a Doctor's own read-only view of the staff assigned to them.
        /// </summary>
        [Authorize(Roles = "Doctor")]
        [HttpGet("assigned-to-me")]
        public async Task<ActionResult<StaffListResponse>> GetAssignedToMe([FromQuery] string? searchTerm)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new GetAssignedStaffQuery(keycloakId, searchTerm));
            return Ok(result);
        }
    }
}
