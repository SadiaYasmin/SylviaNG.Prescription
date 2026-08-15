using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Medicines.Commands.UpdateMedicine;
using SylviaNG.Prescription.Application.Features.Medicines.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Tests.Handlers.Medicines;

public class UpdateMedicineHandlerTests
{
    private readonly Mock<IMedicineRepository> _medicineRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly UpdateMedicineHandler _handler;

    public UpdateMedicineHandlerTests()
    {
        _handler = new UpdateMedicineHandler(_medicineRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidChanges_ShouldUpdateFields()
    {
        var medicine = new Medicine { MedicineId = 1, BrandName = "Napa", Strength = "500mg" };
        _medicineRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(medicine);
        _medicineRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Medicine>().BuildMock());
        var request = new UpdateMedicineRequest { BrandName = "Napa Extra", Strength = "500mg", Manufacturer = "Beximco" };

        var result = await _handler.Handle(new UpdateMedicineCommand(1, request), default);

        result.BrandName.Should().Be("Napa Extra");
        medicine.Manufacturer.Should().Be("Beximco");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentMedicine_ShouldThrowNotFoundException()
    {
        _medicineRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Medicine?)null);

        var act = () => _handler.Handle(new UpdateMedicineCommand(999, new UpdateMedicineRequest { BrandName = "X" }), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenChangeCollidesWithAnotherMedicine_ShouldThrowBadRequestException()
    {
        var medicine = new Medicine { MedicineId = 1, BrandName = "Napa", Strength = "500mg" };
        _medicineRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(medicine);
        _medicineRepositoryMock.Setup(r => r.Query(It.IsAny<bool>()))
            .Returns(new List<Medicine> { new() { MedicineId = 2, BrandName = "Seclo", Strength = "20mg" } }.BuildMock());
        var request = new UpdateMedicineRequest { BrandName = "Seclo", Strength = "20mg" };

        var act = () => _handler.Handle(new UpdateMedicineCommand(1, request), default);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenKeyUnchanged_ShouldNotThrow()
    {
        var medicine = new Medicine { MedicineId = 1, BrandName = "Napa", Strength = "500mg" };
        _medicineRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(medicine);
        _medicineRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Medicine> { medicine }.BuildMock());
        var request = new UpdateMedicineRequest { BrandName = "Napa", Strength = "500mg", Category = "Analgesic" };

        var act = () => _handler.Handle(new UpdateMedicineCommand(1, request), default);

        await act.Should().NotThrowAsync();
    }
}
