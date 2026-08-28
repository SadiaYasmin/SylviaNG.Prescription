using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Features.Doctors.Queries.GetAssignedDoctorDetails;
using SylviaNG.Prescription.Application.Features.Doctors.Queries.GetAssignedDoctors;

namespace SylviaNG.Prescription.Controllers
{
    /// <summary>
    /// A staff member's own read-only "assigned to me" view of doctors — the mirror of
    /// StaffController's "assigned-to-me" endpoint (a doctor's view of their assigned staff).
    /// Kept as its own controller rather than added to the Admin-only DoctorsController, whose
    /// class-level [Authorize(Roles = "Admin")] and doc comment explicitly scope it to the
    /// admin roster/CRUD surface only.
    /// </summary>
    [Authorize(Roles = "Staff")]
    [ApiController]
    [Route("prescription/assigned-doctors")]
    public class AssignedDoctorsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AssignedDoctorsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<AssignedDoctorListResponse>> GetList()
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new GetAssignedDoctorsQuery(keycloakId));
            return Ok(result);
        }

        [HttpGet("{doctorId}")]
        public async Task<ActionResult<AssignedDoctorDetailsResponse>> GetDetails(long doctorId)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new GetAssignedDoctorDetailsQuery(keycloakId, doctorId));
            return Ok(result);
        }
    }
}
