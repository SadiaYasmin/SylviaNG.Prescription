using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Auth.Models;
using SylviaNG.Prescription.Application.Features.Staffs.Commands.CreateStaff;
using SylviaNG.Prescription.Application.Features.Staffs.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Interfaces.Services;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.Tests.TestHelpers;

namespace SylviaNG.Prescription.Tests.Handlers.Staffs;

public class CreateStaffHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CreateStaffHandler _handler;

    public CreateStaffHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Context).Returns(InMemoryDbContextFactory.Create());
        _handler = new CreateStaffHandler(_authServiceMock.Object, _staffRepositoryMock.Object, _doctorRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    private static CreateStaffRequest ValidRequest() => new()
    {
        Username = "new.staff",
        Email = "new.staff@example.com",
        FullName = "Jane Roy",
        Phone = "01712345678",
        Department = "Front Desk",
        AssignedDoctorIds = new List<long> { 10, 20 }
    };

    [Fact]
    public async Task Handle_WithValidRequest_ShouldProvisionAccountAndCreateStaffProfileWithAssignments()
    {
        // Arrange
        var request = ValidRequest();
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Doctor { DoctorId = 10 });
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(20)).ReturnsAsync(new Doctor { DoctorId = 20 });
        _authServiceMock.Setup(a => a.CreateUserAccountAsync(It.Is<CreateUserAccountRequest>(r =>
                r.Username == request.Username && r.Role == UserRoleEnum.Staff)))
            .ReturnsAsync(new CreateUserAccountResponse { UserId = 42, Username = request.Username, TemporaryPassword = "Temp123!" });
        _staffRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Staff>()))
            .Callback<Staff>(s => s.StaffId = 7)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(new CreateStaffCommand(request), default);

        // Assert
        result.StaffId.Should().Be(7);
        result.UserId.Should().Be(42);
        result.TemporaryPassword.Should().Be("Temp123!");
        _staffRepositoryMock.Verify(r => r.AddAsync(It.Is<Staff>(s => s.UserId == 42 && s.FullName == request.FullName)), Times.Once);

        var links = _unitOfWorkMock.Object.Context.ChangeTracker.Entries<StaffDoctor>().Select(e => e.Entity).ToList();
        links.Should().HaveCount(2);
        links.Select(l => l.DoctorId).Should().BeEquivalentTo(new long[] { 10, 20 });
        links.Should().OnlyContain(l => l.StaffId == 7);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WithNonExistentAssignedDoctor_ShouldThrowNotFoundException_WithoutCallingAuthService()
    {
        // Arrange
        var request = ValidRequest();
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((Doctor?)null);

        // Act
        var act = () => _handler.Handle(new CreateStaffCommand(request), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _authServiceMock.Verify(a => a.CreateUserAccountAsync(It.IsAny<CreateUserAccountRequest>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNoAssignedDoctors_ShouldCreateStaffWithoutLinkRows()
    {
        // Arrange
        var request = ValidRequest();
        request.AssignedDoctorIds = new List<long>();
        _authServiceMock.Setup(a => a.CreateUserAccountAsync(It.IsAny<CreateUserAccountRequest>()))
            .ReturnsAsync(new CreateUserAccountResponse { UserId = 42, Username = request.Username, TemporaryPassword = "Temp123!" });
        _staffRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Staff>()))
            .Callback<Staff>(s => s.StaffId = 7)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(new CreateStaffCommand(request), default);

        // Assert
        result.StaffId.Should().Be(7);
        _unitOfWorkMock.Object.Context.ChangeTracker.Entries<StaffDoctor>().Should().BeEmpty();
    }
}
