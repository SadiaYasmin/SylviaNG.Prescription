using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctorPhoto;
using SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctorPreferences;
using SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctorProfile;
using SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctorSignature;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Features.Doctors.Queries.GetDoctorPreferences;
using SylviaNG.Prescription.Application.Features.Doctors.Queries.GetDoctorProfile;

namespace SylviaNG.Prescription.Controllers
{
    /// <summary>
    /// A doctor's own self-service surface: prescription preferences (template/language/
    /// signature, Epic K stub added by Epic D to unblock finalize's US-026 checklist) plus
    /// the full profile/photo endpoints (Epic K proper — US-061/062). Deliberately a separate
    /// controller from DoctorsController (Admin-only roster CRUD, class-level
    /// [Authorize(Roles="Admin")]) — class + method [Authorize] attributes AND together, so a
    /// Doctor-only self-service action can't live on that controller at all. Routes are split
    /// across .../me/preferences and .../me/profile|photo, same controller class.
    /// </summary>
    [Authorize(Roles = "Doctor")]
    [ApiController]
    [Route("prescription/doctors/me")]
    public class DoctorPreferencesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DoctorPreferencesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("preferences")]
        public async Task<ActionResult<DoctorPreferencesResponse>> Get()
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new GetDoctorPreferencesQuery(keycloakId));
            return Ok(result);
        }

        [HttpPut("preferences")]
        public async Task<ActionResult<DoctorPreferencesResponse>> Update([FromBody] UpdateDoctorPreferencesRequest request)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new UpdateDoctorPreferencesCommand(keycloakId, request));
            return Ok(result);
        }

        [HttpPut("preferences/signature")]
        public async Task<ActionResult<DoctorPreferencesResponse>> UpdateSignature([FromBody] UpdateDoctorSignatureRequest request)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new UpdateDoctorSignatureCommand(keycloakId, request));
            return Ok(result);
        }

        [HttpGet("profile")]
        public async Task<ActionResult<DoctorProfileResponse>> GetProfile()
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new GetDoctorProfileQuery(keycloakId));
            return Ok(result);
        }

        [HttpPut("profile")]
        public async Task<ActionResult<DoctorProfileResponse>> UpdateProfile([FromBody] UpdateDoctorProfileRequest request)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new UpdateDoctorProfileCommand(keycloakId, request));
            return Ok(result);
        }

        [HttpPut("photo")]
        public async Task<ActionResult<DoctorProfileResponse>> UpdatePhoto([FromBody] UpdateDoctorPhotoRequest request)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new UpdateDoctorPhotoCommand(keycloakId, request));
            return Ok(result);
        }

        [HttpDelete("photo")]
        public async Task<ActionResult<DoctorProfileResponse>> RemovePhoto()
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new UpdateDoctorPhotoCommand(keycloakId, new UpdateDoctorPhotoRequest { PhotoBase64 = null }));
            return Ok(result);
        }
    }
}
