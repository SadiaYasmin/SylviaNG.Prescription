using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.QuickAdd.Commands.UpdateQuickAddPreset;
using SylviaNG.Prescription.Application.Features.QuickAdd.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Tests.Handlers.QuickAdd;

public class UpdateQuickAddPresetHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IQuickAddPresetRepository> _quickAddPresetRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly UpdateQuickAddPresetHandler _handler;

    public UpdateQuickAddPresetHandlerTests()
    {
        _handler = new UpdateQuickAddPresetHandler(
            _userRepositoryMock.Object, _staffRepositoryMock.Object, _doctorRepositoryMock.Object,
            _quickAddPresetRepositoryMock.Object, _unitOfWorkMock.Object);

        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doc")).ReturnsAsync(
            new User { UserId = 5, KeycloakId = "kc-doc", Role = UserRoleEnum.Doctor, IsActive = true, Username = "doc" });
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(5)).ReturnsAsync(new Doctor { DoctorId = 10, UserId = 5, FullName = "Dr. Doc" });
    }

    [Fact]
    public async Task Handle_WithOwnPreset_ShouldUpdateLabelAndPayload()
    {
        var preset = new QuickAddPreset { QuickAddPresetId = 1, DoctorId = 10, SectionType = QuickAddSectionTypeEnum.Diagnosis, Label = "Old", PayloadJson = "\"Old\"" };
        _quickAddPresetRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(preset);
        var request = new UpdateQuickAddPresetRequest { Label = "New Label", PayloadJson = "\"New Label\"" };

        var result = await _handler.Handle(new UpdateQuickAddPresetCommand("kc-doc", 1, request), default);

        result.Label.Should().Be("New Label");
        preset.PayloadJson.Should().Be("\"New Label\"");
        _quickAddPresetRepositoryMock.Verify(r => r.Update(preset), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithAnotherDoctorsPreset_ShouldThrowNotFoundException()
    {
        var preset = new QuickAddPreset { QuickAddPresetId = 1, DoctorId = 999, SectionType = QuickAddSectionTypeEnum.Diagnosis, Label = "Old", PayloadJson = "\"Old\"" };
        _quickAddPresetRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(preset);
        var request = new UpdateQuickAddPresetRequest { Label = "New", PayloadJson = "\"New\"" };

        var act = () => _handler.Handle(new UpdateQuickAddPresetCommand("kc-doc", 1, request), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _quickAddPresetRepositoryMock.Verify(r => r.Update(It.IsAny<QuickAddPreset>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentPreset_ShouldThrowNotFoundException()
    {
        _quickAddPresetRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((QuickAddPreset?)null);

        var act = () => _handler.Handle(new UpdateQuickAddPresetCommand("kc-doc", 999, new UpdateQuickAddPresetRequest { Label = "X", PayloadJson = "\"X\"" }), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
