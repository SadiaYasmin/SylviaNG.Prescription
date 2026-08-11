using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Templates.Commands.DeleteTemplate;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Tests.Handlers.Templates;

public class DeleteTemplateHandlerTests
{
    private readonly Mock<ITemplateRepository> _templateRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly DeleteTemplateHandler _handler;

    public DeleteTemplateHandlerTests()
    {
        _handler = new DeleteTemplateHandler(_templateRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_DeletingSystemDefaultTemplate_ShouldThrowBadRequestException()
    {
        // Arrange
        var template = new PrescriptionTemplate
        {
            TemplateId = 1,
            Name = "Classic Default",
            Type = TemplateTypeEnum.Classic,
            Language = TemplateLanguageEnum.En,
            Enabled = true,
            IsSystemDefault = true,
            ConfigJson = "{}"
        };
        _templateRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(template);

        // Act
        var act = () => _handler.Handle(new DeleteTemplateCommand(1), default);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.Message.Should().Be("Cannot delete the system default template.");
        _templateRepositoryMock.Verify(r => r.Delete(It.IsAny<PrescriptionTemplate>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_DeletingNonSystemDefaultTemplate_ShouldHardDelete()
    {
        // Arrange
        var template = new PrescriptionTemplate
        {
            TemplateId = 2,
            Name = "Custom",
            Type = TemplateTypeEnum.Corporate,
            Language = TemplateLanguageEnum.En,
            Enabled = true,
            IsSystemDefault = false,
            ConfigJson = "{}"
        };
        _templateRepositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(template);

        // Act
        await _handler.Handle(new DeleteTemplateCommand(2), default);

        // Assert
        _templateRepositoryMock.Verify(r => r.Delete(template), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentTemplate_ShouldThrowNotFoundException()
    {
        // Arrange
        _templateRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((PrescriptionTemplate?)null);

        // Act
        var act = () => _handler.Handle(new DeleteTemplateCommand(999), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
