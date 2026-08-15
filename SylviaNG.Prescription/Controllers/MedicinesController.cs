using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SylviaNG.Prescription.Application.Features.Medicines.Commands.CreateMedicine;
using SylviaNG.Prescription.Application.Features.Medicines.Commands.DeactivateMedicine;
using SylviaNG.Prescription.Application.Features.Medicines.Commands.UpdateMedicine;
using SylviaNG.Prescription.Application.Features.Medicines.Models;
using SylviaNG.Prescription.Application.Features.Medicines.Queries.GetMedicineById;
using SylviaNG.Prescription.Application.Features.Medicines.Queries.GetMedicineCatalog;
using SylviaNG.Prescription.Application.Features.Medicines.Queries.SearchMedicines;

namespace SylviaNG.Prescription.Controllers
{
    /// <summary>
    /// Medicine Catalog (Epic F, US-036-040). <see cref="Search"/> stays cheap and
    /// role-agnostic for the prescription-authoring autocomplete's hot path and Staff's plain
    /// catalog browse; <see cref="GetCatalog"/> is the separate, heavier Admin/Doctor-only
    /// analytics view. Writes are Admin-only.
    /// </summary>
    [ApiController]
    [Route("prescription/medicines")]
    public class MedicinesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MedicinesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = "Admin,Doctor,Staff")]
        [HttpGet]
        public async Task<ActionResult<List<MedicineSummaryResponse>>> Search([FromQuery] string? search)
        {
            var result = await _mediator.Send(new SearchMedicinesQuery(search));
            return Ok(result);
        }

        [Authorize(Roles = "Admin,Doctor")]
        [HttpGet("catalog")]
        public async Task<ActionResult<List<MedicineCatalogResponse>>> GetCatalog([FromQuery] string? search)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new GetMedicineCatalogQuery(search, keycloakId));
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<MedicineCatalogResponse>> GetById(long id)
        {
            var result = await _mediator.Send(new GetMedicineByIdQuery(id));
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<MedicineCatalogResponse>> Create([FromBody] CreateMedicineRequest request)
        {
            var result = await _mediator.Send(new CreateMedicineCommand(request));
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<MedicineCatalogResponse>> Update(long id, [FromBody] UpdateMedicineRequest request)
        {
            var result = await _mediator.Send(new UpdateMedicineCommand(id, request));
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/deactivate")]
        public async Task<ActionResult> Deactivate(long id)
        {
            await _mediator.Send(new DeactivateMedicineCommand(id));
            return Ok();
        }
    }
}
