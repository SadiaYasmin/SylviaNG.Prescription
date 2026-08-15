using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Medicines.Commands.DeactivateMedicine;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Tests.Handlers.Medicines;

public class DeactivateMedicineHandlerTests
{
    private readonly Mock<IMedicineRepository> _medicineRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly DeactivateMedicineHandler _handler;

    public DeactivateMedicineHandlerTests()
    {
        _handler = new DeactivateMedicineHandler(_medicineRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingMedicine_ShouldSetActiveFalse_NotDelete()
    {
        var medicine = new Medicine { MedicineId = 1, BrandName = "Napa", Active = true };
        _medicineRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(medicine);

        await _handler.Handle(new DeactivateMedicineCommand(1), default);

        medicine.Active.Should().BeFalse();
        _medicineRepositoryMock.Verify(r => r.Update(medicine), Times.Once);
        _medicineRepositoryMock.Verify(r => r.Delete(It.IsAny<Medicine>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentMedicine_ShouldThrowNotFoundException()
    {
        _medicineRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Medicine?)null);

        var act = () => _handler.Handle(new DeactivateMedicineCommand(999), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
