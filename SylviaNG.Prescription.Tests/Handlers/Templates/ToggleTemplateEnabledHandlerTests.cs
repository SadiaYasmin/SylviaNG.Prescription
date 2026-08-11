using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Templates.Commands.ToggleTemplateEnabled;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Tests.Handlers.Templates;

public class ToggleTemplateEnabledHandlerTests
{
    private readonly Mock<ITemplateRepository> _templateRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ToggleTemplateEnabledHandler _handler;

    public ToggleTemplateEnabledHandlerTests()
    {
        _handler = new ToggleTemplateEnabledHandler(_templateRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    private static PrescriptionTemplate MakeTemplate(bool enabled, bool isSystemDefault) => new()
    {
        TemplateId = 1,
        Name = "Template",
        Type = TemplateTypeEnum.Classic,
        Language = TemplateLanguageEnum.En,
        Enabled = enabled,
        IsSystemDefault = isSystemDefault,
        ConfigJson = "{}"
    };

    [Fact]
    public async Task Handle_DisablingSystemDefaultTemplate_ShouldThrowBadRequestException()
    {
        // Arrange
        var template = MakeTemplate(enabled: true, isSystemDefault: true);
        _templateRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(template);

        // Act
        var act = () => _handler.Handle(new ToggleTemplateEnabledCommand(1), default);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.Message.Should().Be("Cannot disable the system default template.");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ReEnablingSystemDefaultTemplate_ShouldSucceed()
    {
        // Arrange: re-enabling is always fine, even for the system default.
        var template = MakeTemplate(enabled: false, isSystemDefault: true);
        _templateRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(template);

        // Act
        var result = await _handler.Handle(new ToggleTemplateEnabledCommand(1), default);

        // Assert
        result.Enabled.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_TogglingNonSystemDefaultTemplate_ShouldFlipEnabled()
    {
        // Arrange
        var template = MakeTemplate(enabled: true, isSystemDefault: false);
        _templateRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(template);

        // Act
        var result = await _handler.Handle(new ToggleTemplateEnabledCommand(1), default);

        // Assert
        result.Enabled.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentTemplate_ShouldThrowNotFoundException()
    {
        // Arrange
        _templateRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((PrescriptionTemplate?)null);

        // Act
        var act = () => _handler.Handle(new ToggleTemplateEnabledCommand(999), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
