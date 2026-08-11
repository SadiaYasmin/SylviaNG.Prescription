using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Templates.Commands.UpdateTemplate;
using SylviaNG.Prescription.Application.Features.Templates.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Tests.Handlers.Templates;

public class UpdateTemplateHandlerTests
{
    private readonly Mock<ITemplateRepository> _templateRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly UpdateTemplateHandler _handler;

    public UpdateTemplateHandlerTests()
    {
        _handler = new UpdateTemplateHandler(_templateRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    private static PrescriptionTemplate ExistingTemplate() => new()
    {
        TemplateId = 1,
        Name = "Old Name",
        Type = TemplateTypeEnum.Classic,
        Language = TemplateLanguageEnum.En,
        Enabled = true,
        IsSystemDefault = false,
        ConfigJson = "{}"
    };

    [Fact]
    public async Task Handle_WithExistingTemplate_ShouldUpdateNameAndConfig()
    {
        // Arrange
        var template = ExistingTemplate();
        _templateRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(template);

        var request = new UpdateTemplateRequest
        {
            Name = "New Name",
            Config = new TemplateConfig
            {
                Header = new HeaderConfig { BgColor = "#123456", Height = 120, LogoSize = 60, NameFont = "body", BorderStyle = "dashed" },
                Style = new StyleConfig { FontSize = 16, SectionSpacing = 10, BorderRadius = 4 }
            }
        };

        // Act
        var result = await _handler.Handle(new UpdateTemplateCommand(1, request), default);

        // Assert
        result.Name.Should().Be("New Name");
        result.Config.Header.BgColor.Should().Be("#123456");
        result.Config.Header.Height.Should().Be(120);
        result.Config.Style.FontSize.Should().Be(16);
        _templateRepositoryMock.Verify(r => r.Update(It.Is<PrescriptionTemplate>(t => t.Name == "New Name")), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentTemplate_ShouldThrowNotFoundException()
    {
        // Arrange
        _templateRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((PrescriptionTemplate?)null);

        // Act
        var act = () => _handler.Handle(new UpdateTemplateCommand(999, new UpdateTemplateRequest { Name = "X" }), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
