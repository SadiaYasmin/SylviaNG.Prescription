using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Features.Templates.Queries.GetTemplateList;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Tests.Handlers.Templates;

public class GetTemplateListHandlerTests
{
    private readonly Mock<ITemplateRepository> _templateRepositoryMock = new();
    private readonly GetTemplateListHandler _handler;

    private readonly List<PrescriptionTemplate> _templates = new()
    {
        new PrescriptionTemplate { TemplateId = 1, Name = "Classic Default", Type = TemplateTypeEnum.Classic, Language = TemplateLanguageEnum.En, Enabled = true, IsSystemDefault = true, ConfigJson = "{}" },
        new PrescriptionTemplate { TemplateId = 2, Name = "Corporate Style", Type = TemplateTypeEnum.Corporate, Language = TemplateLanguageEnum.En, Enabled = false, IsSystemDefault = false, ConfigJson = "{}" },
    };

    public GetTemplateListHandlerTests()
    {
        _templateRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(_templates.BuildMock());
        _handler = new GetTemplateListHandler(_templateRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFlatUnpaginatedList()
    {
        // Act
        var result = await _handler.Handle(new GetTemplateListQuery(), default);

        // Assert
        result.Templates.Should().HaveCount(2);
        result.Templates.Should().Contain(t => t.Name == "Classic Default" && t.IsSystemDefault);
        result.Templates.Should().Contain(t => t.Name == "Corporate Style" && !t.Enabled);
    }
}
