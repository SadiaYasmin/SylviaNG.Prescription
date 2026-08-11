using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SylviaNG.Prescription.Application.Features.Templates.Commands.CreateTemplate;
using SylviaNG.Prescription.Application.Features.Templates.Commands.DeleteTemplate;
using SylviaNG.Prescription.Application.Features.Templates.Commands.DuplicateTemplate;
using SylviaNG.Prescription.Application.Features.Templates.Commands.ToggleTemplateEnabled;
using SylviaNG.Prescription.Application.Features.Templates.Commands.UpdateTemplate;
using SylviaNG.Prescription.Application.Features.Templates.Models;
using SylviaNG.Prescription.Application.Features.Templates.Queries.GetTemplateDetails;
using SylviaNG.Prescription.Application.Features.Templates.Queries.GetTemplateList;
using SylviaNG.Prescription.Controllers;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Tests.Controllers;

public class TemplatesControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly TemplatesController _controller;

    public TemplatesControllerTests()
    {
        _controller = new TemplatesController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Create_ShouldReturnOkWithCreatedTemplate()
    {
        var request = new CreateTemplateRequest { Name = "My Template", Type = TemplateTypeEnum.Classic, Language = TemplateLanguageEnum.En };
        var expected = new TemplateDetailsResponse { TemplateId = 1, Name = "My Template" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateTemplateCommand>(), default)).ReturnsAsync(expected);

        var result = await _controller.Create(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Update_ShouldReturnOkWithUpdatedTemplate()
    {
        var request = new UpdateTemplateRequest { Name = "Renamed" };
        var expected = new TemplateDetailsResponse { TemplateId = 1, Name = "Renamed" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateTemplateCommand>(), default)).ReturnsAsync(expected);

        var result = await _controller.Update(1, request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Duplicate_ShouldReturnOkWithClonedTemplate()
    {
        var expected = new TemplateDetailsResponse { TemplateId = 2, Name = "My Template (Copy)", IsSystemDefault = false };
        _mediatorMock.Setup(m => m.Send(It.IsAny<DuplicateTemplateCommand>(), default)).ReturnsAsync(expected);

        var result = await _controller.Duplicate(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task ToggleEnabled_ShouldReturnOkWithToggledTemplate()
    {
        var expected = new TemplateSummaryResponse { TemplateId = 1, Enabled = false };
        _mediatorMock.Setup(m => m.Send(It.IsAny<ToggleTemplateEnabledCommand>(), default)).ReturnsAsync(expected);

        var result = await _controller.ToggleEnabled(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Delete_ShouldReturnOk()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteTemplateCommand>(), default)).ReturnsAsync(Unit.Value);

        var result = await _controller.Delete(1);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetList_ShouldReturnOkWithTemplateList()
    {
        var expected = new TemplateListResponse { Templates = new List<TemplateSummaryResponse>() };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetTemplateListQuery>(), default)).ReturnsAsync(expected);

        var result = await _controller.GetList();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetDetails_ShouldReturnOkWithTemplateDetails()
    {
        var expected = new TemplateDetailsResponse { TemplateId = 1 };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetTemplateDetailsQuery>(), default)).ReturnsAsync(expected);

        var result = await _controller.GetDetails(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }
}
