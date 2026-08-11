using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Templates;
using SylviaNG.Prescription.Application.Features.Templates.Queries.GetTemplateDetails;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Tests.Handlers.Templates;

public class GetTemplateDetailsHandlerTests
{
    private readonly Mock<ITemplateRepository> _templateRepositoryMock = new();
    private readonly GetTemplateDetailsHandler _handler;

    public GetTemplateDetailsHandlerTests()
    {
        _handler = new GetTemplateDetailsHandler(_templateRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingTemplate_ShouldReturnDetailsWithDeserializedConfig()
    {
        // Arrange
        var template = new PrescriptionTemplate { TemplateId = 1, Name = "Classic Default", Type = TemplateTypeEnum.Classic, Language = TemplateLanguageEnum.En, Enabled = true, IsSystemDefault = true };
        template.SetConfig(TemplateDefaults.BuildDefaultConfig(TemplateTypeEnum.Classic, TemplateLanguageEnum.En));
        _templateRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(template);

        // Act
        var result = await _handler.Handle(new GetTemplateDetailsQuery(1), default);

        // Assert
        result.Name.Should().Be("Classic Default");
        result.Config.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentTemplate_ShouldThrowNotFoundException()
    {
        // Arrange
        _templateRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((PrescriptionTemplate?)null);

        // Act
        var act = () => _handler.Handle(new GetTemplateDetailsQuery(999), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
