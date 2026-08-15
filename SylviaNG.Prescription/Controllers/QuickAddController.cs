using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SylviaNG.Prescription.Application.Features.QuickAdd.Commands.AddQuickAddPreset;
using SylviaNG.Prescription.Application.Features.QuickAdd.Commands.DeleteQuickAddPreset;
using SylviaNG.Prescription.Application.Features.QuickAdd.Commands.UpdateQuickAddPreset;
using SylviaNG.Prescription.Application.Features.QuickAdd.Models;
using SylviaNG.Prescription.Application.Features.QuickAdd.Queries.GetAdviceFollowUpPhraseDictionary;
using SylviaNG.Prescription.Application.Features.QuickAdd.Queries.GetQuickAddPresets;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Controllers
{
    /// <summary>
    /// Quick Add presets (Epic G, US-041-044) — full CRUD, scoped to the calling doctor's
    /// own presets. Insertable with one click into authoring (US-025). New doctor sections
    /// self-seed a starter set on first open (see QuickAddPresetSeeder).
    /// </summary>
    [Authorize(Roles = "Doctor")]
    [ApiController]
    [Route("prescription/quick-add")]
    public class QuickAddController : ControllerBase
    {
        private readonly IMediator _mediator;

        public QuickAddController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<QuickAddPresetResponse>>> GetList([FromQuery] QuickAddSectionTypeEnum sectionType)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new GetQuickAddPresetsQuery(keycloakId, sectionType));
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<QuickAddPresetResponse>> Add([FromBody] AddQuickAddPresetRequest request)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new AddQuickAddPresetCommand(keycloakId, request));
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<QuickAddPresetResponse>> Update(long id, [FromBody] UpdateQuickAddPresetRequest request)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new UpdateQuickAddPresetCommand(keycloakId, id, request));
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _mediator.Send(new DeleteQuickAddPresetCommand(keycloakId, id));
            return Ok();
        }

        [HttpGet("advice-phrase-dictionary")]
        public async Task<ActionResult<Dictionary<string, string>>> GetAdvicePhraseDictionary()
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new GetAdviceFollowUpPhraseDictionaryQuery(keycloakId));
            return Ok(result);
        }
    }
}
