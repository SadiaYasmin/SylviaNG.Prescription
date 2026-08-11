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
    /// Admin-only Prescription Template Engine management (Epic H / US-045-052).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("prescription/templates")]
    public class TemplatesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TemplatesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<TemplateDetailsResponse>> Create([FromBody] CreateTemplateRequest request)
        {
            var result = await _mediator.Send(new CreateTemplateCommand(request));
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TemplateDetailsResponse>> Update(long id, [FromBody] UpdateTemplateRequest request)
        {
            var result = await _mediator.Send(new UpdateTemplateCommand(id, request));
            return Ok(result);
        }

        [HttpPost("{id}/duplicate")]
        public async Task<ActionResult<TemplateDetailsResponse>> Duplicate(long id)
        {
            var result = await _mediator.Send(new DuplicateTemplateCommand(id));
            return Ok(result);
        }

        [HttpPatch("{id}/toggle-enabled")]
        public async Task<ActionResult<TemplateSummaryResponse>> ToggleEnabled(long id)
        {
            var result = await _mediator.Send(new ToggleTemplateEnabledCommand(id));
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            await _mediator.Send(new DeleteTemplateCommand(id));
            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<TemplateListResponse>> GetList()
        {
            var result = await _mediator.Send(new GetTemplateListQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TemplateDetailsResponse>> GetDetails(long id)
        {
            var result = await _mediator.Send(new GetTemplateDetailsQuery(id));
            return Ok(result);
        }
    }
}
