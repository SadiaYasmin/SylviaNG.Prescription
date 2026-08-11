using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Features.Templates;
using SylviaNG.Prescription.Application.Features.Templates.Commands.CreateTemplate;
using SylviaNG.Prescription.Application.Features.Templates.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Tests.Handlers.Templates;

public class CreateTemplateHandlerTests
{
    private readonly Mock<ITemplateRepository> _templateRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CreateTemplateHandler _handler;

    public CreateTemplateHandlerTests()
    {
        _handler = new CreateTemplateHandler(_templateRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateTemplateAsEnabledAndNotSystemDefault()
    {
        // Arrange
        var request = new CreateTemplateRequest { Name = "My Template", Type = TemplateTypeEnum.Corporate, Language = TemplateLanguageEnum.Bn };
        PrescriptionTemplate? captured = null;
        _templateRepositoryMock.Setup(r => r.AddAsync(It.IsAny<PrescriptionTemplate>()))
            .Callback<PrescriptionTemplate>(t => { t.TemplateId = 10; captured = t; })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(new CreateTemplateCommand(request), default);

        // Assert
        result.TemplateId.Should().Be(10);
        result.Name.Should().Be("My Template");
        result.Enabled.Should().BeTrue();
        result.IsSystemDefault.Should().BeFalse();
        captured.Should().NotBeNull();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSeedConfigFromTemplateDefaults_ForGivenTypeAndLanguage()
    {
        // Arrange: config seeded from TemplateDefaults for the requested type+language (US-046) —
        // the client does NOT supply config on create.
        var request = new CreateTemplateRequest { Name = "Gov Template", Type = TemplateTypeEnum.Government, Language = TemplateLanguageEnum.Bn };
        _templateRepositoryMock.Setup(r => r.AddAsync(It.IsAny<PrescriptionTemplate>())).Returns(Task.CompletedTask);

        var expectedDefaults = TemplateDefaults.BuildDefaultConfig(TemplateTypeEnum.Government, TemplateLanguageEnum.Bn);

        // Act
        var result = await _handler.Handle(new CreateTemplateCommand(request), default);

        // Assert
        result.Config.Header.Height.Should().Be(expectedDefaults.Header.Height);
        result.Config.Style.AccentColor.Should().Be(expectedDefaults.Style.AccentColor);
        result.Config.Labels.Should().BeEquivalentTo(expectedDefaults.Labels);
        result.Config.Labels["prescriptionHeading"].Should().Be("Rx");
    }
}
