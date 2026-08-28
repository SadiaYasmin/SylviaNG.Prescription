using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SylviaNG.Prescription.Application.Features.Analytics;
using SylviaNG.Prescription.Application.Features.Doctors.Commands.CreateDoctor;
using SylviaNG.Prescription.Application.Features.Doctors.Commands.DeactivateDoctor;
using SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctor;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Features.Doctors.Queries.GetDoctorDetails;
using SylviaNG.Prescription.Application.Features.Doctors.Queries.GetDoctorList;

namespace SylviaNG.Prescription.Controllers
{
    /// <summary>
    /// Admin-only Doctor Management (Epic I / US-053–056). Doctors manage their own
    /// profile/settings through a separate endpoint (Epic K) — this controller is
    /// exclusively the admin roster/CRUD surface.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("prescription/doctors")]
    public class DoctorsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DoctorsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<CreateDoctorResponse>> Create([FromBody] CreateDoctorRequest request)
        {
            var result = await _mediator.Send(new CreateDoctorCommand(request));
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DoctorSummaryResponse>> Update(long id, [FromBody] UpdateDoctorRequest request)
        {
            var result = await _mediator.Send(new UpdateDoctorCommand(id, request));
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Deactivate(long id)
        {
            await _mediator.Send(new DeactivateDoctorCommand(id));
            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<DoctorListResponse>> GetList([FromQuery] DoctorListRequest request)
        {
            var result = await _mediator.Send(new GetDoctorListQuery(request));
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DoctorDetailsResponse>> GetDetails(
            long id,
            [FromQuery] AnalyticsGranularity activityGranularity = AnalyticsGranularity.Day)
        {
            var result = await _mediator.Send(new GetDoctorDetailsQuery(id, activityGranularity));
            return Ok(result);
        }
    }
}
