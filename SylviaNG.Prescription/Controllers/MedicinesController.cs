using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SylviaNG.Prescription.Application.Features.Medicines.Models;
using SylviaNG.Prescription.Application.Features.Medicines.Queries.SearchMedicines;

namespace SylviaNG.Prescription.Controllers
{
    /// <summary>
    /// Medicine Catalog (Epic F stub) — read-only search feeding the Prescription authoring
    /// autocomplete (US-022) and the standalone catalog browse (US-036). No admin CRUD yet.
    /// </summary>
    [Authorize(Roles = "Admin,Doctor,Staff")]
    [ApiController]
    [Route("prescription/medicines")]
    public class MedicinesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MedicinesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<MedicineSummaryResponse>>> Search([FromQuery] string? search)
        {
            var result = await _mediator.Send(new SearchMedicinesQuery(search));
            return Ok(result);
        }
    }
}
