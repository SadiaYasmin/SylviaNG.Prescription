using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Services;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.Tests.TestHelpers;

namespace SylviaNG.Prescription.Tests.Services;

public class QuickAddPresetSeederTests
{
    private readonly Mock<IQuickAddPresetRepository> _quickAddPresetRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ApplicationDBContext _context = InMemoryDbContextFactory.Create();
    private readonly QuickAddPresetSeeder _seeder;

    private readonly List<QuickAddPreset> _presets = new();

    public QuickAddPresetSeederTests()
    {
        _quickAddPresetRepositoryMock.Setup(r => r.AddAsync(It.IsAny<QuickAddPreset>()))
            .Callback<QuickAddPreset>(p => _presets.Add(p))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.Context).Returns(_context);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(() => _context.SaveChangesAsync());

        _seeder = new QuickAddPresetSeeder(_quickAddPresetRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task SeedIfNeededAsync_OnFirstOpenOfASection_ShouldInsertStartersAndWriteSeedState()
    {
        await _seeder.SeedIfNeededAsync(1, QuickAddSectionTypeEnum.Advice, default);

        _presets.Should().NotBeEmpty();
        _presets.Should().OnlyContain(p => p.DoctorId == 1 && p.SectionType == QuickAddSectionTypeEnum.Advice);
        _context.DoctorQuickAddSeedStates.Should().ContainSingle(s => s.DoctorId == 1 && s.SectionType == QuickAddSectionTypeEnum.Advice);
    }

    [Fact]
    public async Task SeedIfNeededAsync_CalledTwice_ShouldOnlySeedOnce()
    {
        await _seeder.SeedIfNeededAsync(1, QuickAddSectionTypeEnum.Diagnosis, default);
        var firstCount = _presets.Count;

        await _seeder.SeedIfNeededAsync(1, QuickAddSectionTypeEnum.Diagnosis, default);

        _presets.Should().HaveCount(firstCount);
    }

    [Fact]
    public async Task SeedIfNeededAsync_ForADoctorWhoAlreadyHasASeedStateRow_ShouldNotReSeedEvenIfPresetsWereAllDeleted()
    {
        // Simulates a doctor who deliberately emptied a previously-seeded section — the
        // seed-state row is the only thing that must matter, not the current preset count.
        _context.DoctorQuickAddSeedStates.Add(new DoctorQuickAddSeedState
        {
            DoctorId = 1,
            SectionType = QuickAddSectionTypeEnum.Investigation,
            SeededAt = DateTime.UtcNow
        });
        _context.SaveChanges();

        await _seeder.SeedIfNeededAsync(1, QuickAddSectionTypeEnum.Investigation, default);

        _presets.Should().BeEmpty();
    }

    [Fact]
    public async Task SeedIfNeededAsync_IsScopedPerDoctorAndPerSection()
    {
        await _seeder.SeedIfNeededAsync(1, QuickAddSectionTypeEnum.Medicine, default);

        await _seeder.SeedIfNeededAsync(2, QuickAddSectionTypeEnum.Medicine, default);
        await _seeder.SeedIfNeededAsync(1, QuickAddSectionTypeEnum.FollowUp, default);

        _context.DoctorQuickAddSeedStates.Should().HaveCount(3);
    }
}
