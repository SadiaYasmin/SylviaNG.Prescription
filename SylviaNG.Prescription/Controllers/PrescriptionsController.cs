using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SylviaNG.Prescription.Application.Features.Prescriptions.Commands.AutoSavePrescription;
using SylviaNG.Prescription.Application.Features.Prescriptions.Commands.FinalizePrescription;
using SylviaNG.Prescription.Application.Features.Prescriptions.Commands.SaveDraftPrescription;
using SylviaNG.Prescription.Application.Features.Prescriptions.Commands.StartOrResumePrescription;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;
using SylviaNG.Prescription.Application.Features.Prescriptions.Queries.GetMyDraftPrescriptions;
using SylviaNG.Prescription.Application.Features.Prescriptions.Queries.GetMyFinalizedPrescriptions;
using SylviaNG.Prescription.Application.Features.Prescriptions.Queries.GetPatientPrescriptionHistory;
using SylviaNG.Prescription.Application.Features.Prescriptions.Queries.GetPrescriptionDetails;
using SylviaNG.Prescription.Application.Features.Prescriptions.Queries.VerifyPrescription;

namespace SylviaNG.Prescription.Controllers
{
    /// <summary>
    /// Prescription Authoring + Lifecycle (Epic D/E). No class-level [Authorize] — the
    /// Verify action is public (US-035), authoring/finalize/lists are Doctor-only, and the
    /// single-view/patient-history reads are open to Doctor/Staff/Admin (each narrowed
    /// server-side by PrescriptionVisibilityScope) — same per-action pattern as
    /// ConsultationsController/StaffController.
    /// </summary>
    [ApiController]
    [Route("prescription/prescriptions")]
    public class PrescriptionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PrescriptionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = "Doctor")]
        [HttpPost("start")]
        public async Task<ActionResult<StartOrResumePrescriptionResponse>> Start([FromBody] StartOrResumePrescriptionRequest request)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new StartOrResumePrescriptionCommand(keycloakId, request));
            return Ok(result);
        }

        [Authorize(Roles = "Doctor")]
        [HttpPut("{id}")]
        public async Task<ActionResult<PrescriptionDocumentResponse>> SaveDraft(long id, [FromBody] SaveDraftPrescriptionRequest request)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new SaveDraftPrescriptionCommand(keycloakId, id, request));
            return Ok(result);
        }

        [Authorize(Roles = "Doctor")]
        [HttpPatch("{id}/autosave")]
        public async Task<ActionResult<AutoSavePrescriptionResponse>> AutoSave(long id, [FromBody] SaveDraftPrescriptionRequest request)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new AutoSavePrescriptionCommand(keycloakId, id, request));
            return Ok(result);
        }

        [Authorize(Roles = "Doctor")]
        [HttpPost("{id}/finalize")]
        public async Task<ActionResult<PrescriptionDocumentResponse>> Finalize(long id, [FromBody] SaveDraftPrescriptionRequest request)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new FinalizePrescriptionCommand(keycloakId, id, request));
            return Ok(result);
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("drafts")]
        public async Task<ActionResult<PrescriptionListResponse>> GetDrafts(
            [FromQuery] long? patientId,
            [FromQuery] string? searchTerm,
            [FromQuery] DateOnly? date,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new GetMyDraftPrescriptionsQuery(keycloakId, patientId, searchTerm, date, page, pageSize));
            return Ok(result);
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("finalized")]
        public async Task<ActionResult<PrescriptionListResponse>> GetFinalized(
            [FromQuery] string? searchTerm,
            [FromQuery] DateOnly? fromDate,
            [FromQuery] DateOnly? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new GetMyFinalizedPrescriptionsQuery(keycloakId, searchTerm, fromDate, toDate, page, pageSize));
            return Ok(result);
        }

        [Authorize(Roles = "Doctor,Staff,Admin")]
        [HttpGet("patient/{patientId}/history")]
        public async Task<ActionResult<PrescriptionListResponse>> GetPatientHistory(long patientId)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new GetPatientPrescriptionHistoryQuery(keycloakId, patientId));
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("verify/{displayCode}")]
        public async Task<ActionResult<PrescriptionDocumentResponse>> Verify(string displayCode)
        {
            var result = await _mediator.Send(new VerifyPrescriptionQuery(displayCode));
            return Ok(result);
        }

        [Authorize(Roles = "Doctor,Staff,Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<PrescriptionDocumentResponse>> GetDetails(long id)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new GetPrescriptionDetailsQuery(keycloakId, id));
            return Ok(result);
        }
    }
}
