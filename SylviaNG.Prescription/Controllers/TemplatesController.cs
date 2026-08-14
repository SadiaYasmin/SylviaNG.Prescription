using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SylviaNG.Prescription.Application.Features.Templates.Commands.CreateTemplate;
using SylviaNG.Prescription.Application.Features.Templates.Commands.DeleteTemplate;
using SylviaNG.Prescription.Application.Features.Templates.Commands.DuplicateTemplate;
using SylviaNG.Prescription.Application.Features.Templates.Commands.ToggleTemplateEnabled;
using SylviaNG.Prescription.Application.Features.Templates.Commands.UpdateTemplate;
using SylviaNG.Prescription.Application.Features.Templates.Models;
using SylviaNG.Prescription.Application.Features.Templates.Queries.GetTemplateDetails;
using SylviaNG.Prescription.Application.Features.Templates.Queries.GetTemplateList;

namespace SylviaNG.Prescription.Controllers
{
    /// <summary>
    /// Prescription Template Engine management (Epic H / US-045-052) — Admin-only for every
    /// write action. The two GET (read) actions were opened to Doctor too (Epic D) so a
    /// doctor can list enabled templates and read one's config for the preferred-template
    /// picker (US-064) — no class-level [Authorize] any more, per-action instead, same
    /// reasoning as ConsultationsController.
    /// </summary>
    [ApiController]
    [Route("prescription/templates")]
    public class TemplatesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TemplatesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<TemplateDetailsResponse>> Create([FromBody] CreateTemplateRequest request)
        {
            var result = await _mediator.Send(new CreateTemplateCommand(request));
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<TemplateDetailsResponse>> Update(long id, [FromBody] UpdateTemplateRequest request)
        {
            var result = await _mediator.Send(new UpdateTemplateCommand(id, request));
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/duplicate")]
        public async Task<ActionResult<TemplateDetailsResponse>> Duplicate(long id)
        {
            var result = await _mediator.Send(new DuplicateTemplateCommand(id));
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/toggle-enabled")]
        public async Task<ActionResult<TemplateSummaryResponse>> ToggleEnabled(long id)
        {
            var result = await _mediator.Send(new ToggleTemplateEnabledCommand(id));
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            await _mediator.Send(new DeleteTemplateCommand(id));
            return Ok();
        }

        [Authorize(Roles = "Admin,Doctor")]
        [HttpGet]
        public async Task<ActionResult<TemplateListResponse>> GetList([FromQuery] bool enabledOnly = false)
        {
            var result = await _mediator.Send(new GetTemplateListQuery(enabledOnly));
            return Ok(result);
        }

        [Authorize(Roles = "Admin,Doctor")]
        [HttpGet("{id}")]
        public async Task<ActionResult<TemplateDetailsResponse>> GetDetails(long id)
        {
            var result = await _mediator.Send(new GetTemplateDetailsQuery(id));
            return Ok(result);
        }
    }
}
