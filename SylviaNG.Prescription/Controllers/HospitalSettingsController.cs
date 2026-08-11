using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SylviaNG.Prescription.Application.Features.HospitalSettings.Commands.UpdateHospitalSettings;
using SylviaNG.Prescription.Application.Features.HospitalSettings.Models;
using SylviaNG.Prescription.Application.Features.HospitalSettings.Queries.GetHospitalSettings;

namespace SylviaNG.Prescription.Controllers
{
    /// <summary>
    /// Hospital identity/branding (Epic H / US-045). No class-level [Authorize] — Get is
    /// available to Admin and Doctor (doctors need it to render prescriptions), Update is
    /// Admin-only. Mirrors StaffController's per-action authorization pattern.
    /// </summary>
    [ApiController]
    [Route("prescription/hospital-settings")]
    public class HospitalSettingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public HospitalSettingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = "Admin,Doctor")]
        [HttpGet]
        public async Task<ActionResult<HospitalSettingsResponse>> Get()
        {
            var result = await _mediator.Send(new GetHospitalSettingsQuery());
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<ActionResult<HospitalSettingsResponse>> Update([FromBody] UpdateHospitalSettingsRequest request)
        {
            var result = await _mediator.Send(new UpdateHospitalSettingsCommand(request));
            return Ok(result);
        }
    }
}
