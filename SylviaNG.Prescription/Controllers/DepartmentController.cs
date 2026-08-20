using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SylviaNG.Prescription.Application.Features.Departments.Commands.CreateDepartment;
using SylviaNG.Prescription.Application.Features.Departments.Commands.DeactivateDepartment;
using SylviaNG.Prescription.Application.Features.Departments.Commands.UpdateDepartment;
using SylviaNG.Prescription.Application.Features.Departments.Models;
using SylviaNG.Prescription.Application.Features.Departments.Queries.GetDepartmentList;

namespace SylviaNG.Prescription.Controllers
{
    /// <summary>
    /// Admin-managed Department list, used to populate the Department dropdown on the
    /// Doctor/Staff create/edit forms and My Profile. Read access is open to any
    /// authenticated role (a Doctor/Staff editing their own profile needs the option list
    /// too); mutating it is Admin-only.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("prescription/departments")]
    public class DepartmentController : ControllerBase
    {
        private readonly IMediator _mediator;
        public DepartmentController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<ActionResult<List<DepartmentResponse>>> GetAll()
            => Ok(await _mediator.Send(new GetDepartmentListQuery()));

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<long>> Create([FromBody] DepartmentCreateRequest request)
            => Ok(await _mediator.Send(new CreateDepartmentCommand(request)));

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(long id, [FromBody] DepartmentUpdateRequest request)
        {
            await _mediator.Send(new UpdateDepartmentCommand(id, request));
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Deactivate(long id)
        {
            await _mediator.Send(new DeactivateDepartmentCommand(id));
            return Ok();
        }
    }
}
