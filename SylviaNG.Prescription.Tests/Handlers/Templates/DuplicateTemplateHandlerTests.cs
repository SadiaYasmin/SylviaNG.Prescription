using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Templates.Commands.DuplicateTemplate;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Tests.Handlers.Templates;

public class DuplicateTemplateHandlerTests
{
    private readonly Mock<ITemplateRepository> _templateRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly DuplicateTemplateHandler _handler;

    public DuplicateTemplateHandlerTests()
    {
        _handler = new DuplicateTemplateHandler(_templateRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithSystemDefaultSource_ShouldProduceCloneThatIsNotSystemDefault()
    {
        // Arrange: duplicating always produces IsSystemDefault = false, even when cloning
        // the system default template itself (US-050).
        var source = new PrescriptionTemplate
        {
            TemplateId = 1,
            Name = "Classic Default",
            Type = TemplateTypeEnum.Classic,
            Language = TemplateLanguageEnum.En,
            Enabled = true,
            IsSystemDefault = true,
            ConfigJson = "{\"Header\":{}}"
        };
        _templateRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(source);
        PrescriptionTemplate? captured = null;
        _templateRepositoryMock.Setup(r => r.AddAsync(It.IsAny<PrescriptionTemplate>()))
            .Callback<PrescriptionTemplate>(t => { t.TemplateId = 2; captured = t; })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(new DuplicateTemplateCommand(1), default);

        // Assert
        result.IsSystemDefault.Should().BeFalse();
        result.Name.Should().Be("Classic Default (Copy)");
        captured!.IsSystemDefault.Should().BeFalse();
        captured.ConfigJson.Should().Be(source.ConfigJson);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentTemplate_ShouldThrowNotFoundException()
    {
        // Arrange
        _templateRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((PrescriptionTemplate?)null);

        // Act
        var act = () => _handler.Handle(new DuplicateTemplateCommand(999), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
