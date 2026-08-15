using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.HospitalSettings.Commands.UpdateHospitalSettings;
using SylviaNG.Prescription.Application.Features.HospitalSettings.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Interfaces.Services;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Tests.Handlers.HospitalSettings;

public class UpdateHospitalSettingsHandlerTests
{
    private readonly Mock<IHospitalSettingsRepository> _hospitalSettingsRepositoryMock = new();
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly UpdateHospitalSettingsHandler _handler;

    public UpdateHospitalSettingsHandlerTests()
    {
        _handler = new UpdateHospitalSettingsHandler(
            _hospitalSettingsRepositoryMock.Object, _fileStorageServiceMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingRow_ShouldUpdateWhicheverIdItActuallyHas()
    {
        // Arrange: fetched via .FirstOrDefaultAsync(), never a hardcoded id=1.
        var existing = new List<Domain.Entities.HospitalSettings>
        {
            new() { HospitalSettingsId = 7, Name = "Old Name", Address = "Old Address", Phone = "01711111111" }
        };
        _hospitalSettingsRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(existing.BuildMock());

        var request = new UpdateHospitalSettingsRequest
        {
            Name = "New Hospital Name",
            Address = "New Address",
            Phone = "01799999999",
            Email = "contact@hospital.com"
        };

        // Act
        var result = await _handler.Handle(new UpdateHospitalSettingsCommand(request), default);

        // Assert
        result.HospitalSettingsId.Should().Be(7);
        result.Name.Should().Be("New Hospital Name");
        result.Email.Should().Be("contact@hospital.com");
        _hospitalSettingsRepositoryMock.Verify(r => r.Update(It.Is<Domain.Entities.HospitalSettings>(h => h.HospitalSettingsId == 7 && h.Name == "New Hospital Name")), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoRow_ShouldThrowNotFoundException()
    {
        // Arrange
        var existing = new List<Domain.Entities.HospitalSettings>();
        _hospitalSettingsRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(existing.BuildMock());

        // Act
        var act = () => _handler.Handle(new UpdateHospitalSettingsCommand(new UpdateHospitalSettingsRequest()), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
