using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Medicines.Commands.CreateMedicine;
using SylviaNG.Prescription.Application.Features.Medicines.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Tests.Handlers.Medicines;

public class CreateMedicineHandlerTests
{
    private readonly Mock<IMedicineRepository> _medicineRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CreateMedicineHandler _handler;

    public CreateMedicineHandlerTests()
    {
        _handler = new CreateMedicineHandler(_medicineRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    private void SetUpExisting(params Medicine[] medicines)
    {
        _medicineRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(medicines.BuildMock());
    }

    [Fact]
    public async Task Handle_WithNewBrandAndStrength_ShouldCreateMedicine()
    {
        SetUpExisting();
        var request = new CreateMedicineRequest { BrandName = "Napa", Strength = "500mg", GenericName = "Paracetamol" };

        var result = await _handler.Handle(new CreateMedicineCommand(request), default);

        result.BrandName.Should().Be("Napa");
        result.TotalPrescribed.Should().Be(0);
        _medicineRepositoryMock.Verify(r => r.AddAsync(It.Is<Medicine>(m => m.BrandName == "Napa" && m.Active)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateBrandAndStrength_ShouldThrowBadRequestException()
    {
        SetUpExisting(new Medicine { MedicineId = 1, BrandName = "Napa", Strength = "500mg" });
        var request = new CreateMedicineRequest { BrandName = "napa", Strength = " 500MG " };

        var act = () => _handler.Handle(new CreateMedicineCommand(request), default);

        await act.Should().ThrowAsync<BadRequestException>();
        _medicineRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Medicine>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithSameBrandDifferentStrength_ShouldNotThrow()
    {
        SetUpExisting(new Medicine { MedicineId = 1, BrandName = "Napa", Strength = "500mg" });
        var request = new CreateMedicineRequest { BrandName = "Napa", Strength = "250mg" };

        var act = () => _handler.Handle(new CreateMedicineCommand(request), default);

        await act.Should().NotThrowAsync();
    }
}
