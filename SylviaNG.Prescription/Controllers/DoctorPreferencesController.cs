using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctorPreferences;
using SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctorSignature;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Features.Doctors.Queries.GetDoctorPreferences;

namespace SylviaNG.Prescription.Controllers
{
    /// <summary>
    /// A doctor's own prescription preferences (Epic K stub, added by Epic D to unblock
    /// finalize's US-026 checklist). Deliberately a separate controller from
    /// DoctorsController (Admin-only roster CRUD, class-level [Authorize(Roles="Admin")]) —
    /// class + method [Authorize] attributes AND together, so a Doctor-only self-service
    /// action can't live on that controller at all.
    /// </summary>
    [Authorize(Roles = "Doctor")]
    [ApiController]
    [Route("prescription/doctors/me/preferences")]
    public class DoctorPreferencesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DoctorPreferencesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<DoctorPreferencesResponse>> Get()
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new GetDoctorPreferencesQuery(keycloakId));
            return Ok(result);
        }

        [HttpPut]
        public async Task<ActionResult<DoctorPreferencesResponse>> Update([FromBody] UpdateDoctorPreferencesRequest request)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new UpdateDoctorPreferencesCommand(keycloakId, request));
            return Ok(result);
        }

        [HttpPut("signature")]
        public async Task<ActionResult<DoctorPreferencesResponse>> UpdateSignature([FromBody] UpdateDoctorSignatureRequest request)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new UpdateDoctorSignatureCommand(keycloakId, request));
            return Ok(result);
        }
    }
}
