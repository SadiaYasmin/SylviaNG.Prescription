using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SylviaNG.Prescription.Application.Features.Consultations.Commands.CreateConsultation;
using SylviaNG.Prescription.Application.Features.Consultations.Commands.OpenConsultation;
using SylviaNG.Prescription.Application.Features.Consultations.Models;
using SylviaNG.Prescription.Application.Features.Consultations.Queries.GetConsultationDetails;
using SylviaNG.Prescription.Application.Features.Consultations.Queries.GetConsultationList;
using SylviaNG.Prescription.Application.Features.Consultations.Queries.GetMyAssignedDoctors;
using SylviaNG.Prescription.Application.Features.Consultations.Queries.GetMyQueue;
using SylviaNG.Prescription.Application.Features.Consultations.Queries.GetTodaysQueue;

namespace SylviaNG.Prescription.Controllers
{
    /// <summary>
    /// Consultation & Queue Management (Epic C). Three distinct roles each get a narrow
    /// slice of this controller — Staff check patients in and see their own queue, Doctor
    /// works their own same-day queue, Admin gets the full roster + details — so, same as
    /// PatientsController/StaffController, there's no class-level [Authorize].
    /// </summary>
    [ApiController]
    [Route("prescription/consultations")]
    public class ConsultationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ConsultationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = "Staff")]
        [HttpPost]
        public async Task<ActionResult<CreateConsultationResponse>> Create([FromBody] CreateConsultationRequest request)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new CreateConsultationCommand(keycloakId, request));
            return Ok(result);
        }

        [Authorize(Roles = "Doctor")]
        [HttpPost("{id}/open")]
        public async Task<ActionResult<OpenConsultationResponse>> Open(long id)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new OpenConsultationCommand(id, keycloakId));
            return Ok(result);
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("today-queue")]
        public async Task<ActionResult<List<QueueItemResponse>>> GetTodaysQueue()
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new GetTodaysQueueQuery(keycloakId));
            return Ok(result);
        }

        [Authorize(Roles = "Staff")]
        [HttpGet("my-queue")]
        public async Task<ActionResult<List<QueueItemResponse>>> GetMyQueue()
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new GetMyQueueQuery(keycloakId));
            return Ok(result);
        }

        [Authorize(Roles = "Staff")]
        [HttpGet("my-assigned-doctors")]
        public async Task<ActionResult<List<AssignedDoctorSummaryResponse>>> GetMyAssignedDoctors()
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new GetMyAssignedDoctorsQuery(keycloakId));
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<ConsultationListResponse>> GetList([FromQuery] ConsultationListRequest request)
        {
            var result = await _mediator.Send(new GetConsultationListQuery(request));
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ConsultationDetailsResponse>> GetDetails(long id)
        {
            var result = await _mediator.Send(new GetConsultationDetailsQuery(id));
            return Ok(result);
        }
    }
}
