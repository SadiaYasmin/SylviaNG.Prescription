using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SylviaNG.Prescription.Application.Features.Patients.Commands.CreatePatient;
using SylviaNG.Prescription.Application.Features.Patients.Commands.UpdatePatient;
using SylviaNG.Prescription.Application.Features.Patients.Models;
using SylviaNG.Prescription.Application.Features.Patients.Queries.GetPatientDetails;
using SylviaNG.Prescription.Application.Features.Patients.Queries.GetPatientList;

namespace SylviaNG.Prescription.Controllers
{
    /// <summary>
    /// Patient Management (Epic B). A Patient is never 1:1 with a User — patients don't log
    /// in and have no Keycloak account — so unlike Doctor/Staff there's no account
    /// provisioning here, just a profile row plus who registered it. Visibility is
    /// role-scoped: a Staff member sees only patients they registered, a Doctor sees
    /// patients registered by any Staff member assigned to them (via Epic J's StaffDoctor
    /// join), and Admin sees everyone — see PatientVisibilityScope for the shared logic.
    /// Per-action [Authorize] (no class-level attribute) since roles differ per action,
    /// same reasoning as StaffController.
    /// </summary>
    [ApiController]
    [Route("prescription/patients")]
    public class PatientsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PatientsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = "Staff")]
        [HttpPost]
        public async Task<ActionResult<PatientSummaryResponse>> Create([FromBody] CreatePatientRequest request)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new CreatePatientCommand(keycloakId, request));
            return Ok(result);
        }

        [Authorize(Roles = "Staff,Doctor")]
        [HttpPut("{id}")]
        public async Task<ActionResult<PatientSummaryResponse>> Update(long id, [FromBody] UpdatePatientRequest request)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new UpdatePatientCommand(id, keycloakId, request));
            return Ok(result);
        }

        [Authorize(Roles = "Staff,Doctor,Admin")]
        [HttpGet]
        public async Task<ActionResult<PatientListResponse>> GetList([FromQuery] PatientListRequest request)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new GetPatientListQuery(keycloakId, request));
            return Ok(result);
        }

        [Authorize(Roles = "Staff,Doctor,Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<PatientDetailsResponse>> GetDetails(long id)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new GetPatientDetailsQuery(id, keycloakId));
            return Ok(result);
        }
    }
}
