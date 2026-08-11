using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Services;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Tests.Services;

public class TemplateEngineSeederTests
{
    private readonly Mock<ITemplateRepository> _templateRepositoryMock = new();
    private readonly Mock<IHospitalSettingsRepository> _hospitalSettingsRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<TemplateEngineSeeder>> _loggerMock = new();
    private readonly TemplateEngineSeeder _seeder;

    private readonly List<PrescriptionTemplate> _templates = new();
    private readonly List<HospitalSettings> _hospitalSettingsRows = new();

    public TemplateEngineSeederTests()
    {
        _templateRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(() => _templates.AsEnumerable());
        _templateRepositoryMock.Setup(r => r.AddAsync(It.IsAny<PrescriptionTemplate>()))
            .Callback<PrescriptionTemplate>(t => _templates.Add(t))
            .Returns(Task.CompletedTask);

        _hospitalSettingsRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(() => _hospitalSettingsRows.AsEnumerable());
        _hospitalSettingsRepositoryMock.Setup(r => r.AddAsync(It.IsAny<HospitalSettings>()))
            .Callback<HospitalSettings>(h => _hospitalSettingsRows.Add(h))
            .Returns(Task.CompletedTask);

        _seeder = new TemplateEngineSeeder(
            _templateRepositoryMock.Object,
            _hospitalSettingsRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task SeedAsync_OnEmptyDatabase_ShouldInsertThreeBaselineTemplatesAndOneHospitalSettingsRow()
    {
        // Act
        await _seeder.SeedAsync();

        // Assert
        _templates.Should().HaveCount(3);
        _templates.Should().OnlyContain(t => t.Language == TemplateLanguageEnum.En && t.Enabled && !string.IsNullOrWhiteSpace(t.ConfigJson));

        var systemDefault = _templates.Should().ContainSingle(t => t.IsSystemDefault).Which;
        systemDefault.Name.Should().Be("Classic Bangladesh Chamber");
        systemDefault.Type.Should().Be(TemplateTypeEnum.Classic);

        _templates.Should().Contain(t => t.Name == "Modern Corporate Hospital" && t.Type == TemplateTypeEnum.Corporate && !t.IsSystemDefault);
        _templates.Should().Contain(t => t.Name == "Government Hospital" && t.Type == TemplateTypeEnum.Government && !t.IsSystemDefault);

        _hospitalSettingsRows.Should().ContainSingle();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SeedAsync_CalledTwice_ShouldStayIdempotent_NoDuplicateSeeding()
    {
        // Act
        await _seeder.SeedAsync();
        await _seeder.SeedAsync();

        // Assert
        _templates.Should().HaveCount(3);
        _templates.Should().ContainSingle(t => t.IsSystemDefault);
        _hospitalSettingsRows.Should().ContainSingle();

        // Second call finds both already seeded, so it never touches SaveChangesAsync again.
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SeedAsync_WhenTemplatesAlreadyExist_ShouldStillSeedHospitalSettingsIfMissing()
    {
        // Arrange: simulate a partially-seeded database (templates present, settings absent).
        _templates.Add(new PrescriptionTemplate { TemplateId = 1, Name = "Existing", IsSystemDefault = true, ConfigJson = "{}" });

        // Act
        await _seeder.SeedAsync();

        // Assert
        _templates.Should().ContainSingle();
        _hospitalSettingsRows.Should().ContainSingle();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
