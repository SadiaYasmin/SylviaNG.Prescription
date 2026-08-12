using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Consultations.Queries.GetConsultationDetails;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Tests.Handlers.Consultations;

public class GetConsultationDetailsHandlerTests
{
    private readonly Mock<IConsultationRepository> _consultationRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly GetConsultationDetailsHandler _handler;

    public GetConsultationDetailsHandlerTests()
    {
        _handler = new GetConsultationDetailsHandler(
            _consultationRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _staffRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingConsultation_ShouldReturnJoinedDetails()
    {
        // Arrange
        var consultation = new Consultation
        {
            ConsultationId = 1,
            PatientId = 1,
            DoctorId = 10,
            RegisteredByStaffId = 3,
            VisitDate = new DateOnly(2026, 8, 11),
            Status = ConsultationStatusEnum.Waiting,
            DisplayCode = "CN-2026-0001",
            TokenNumber = "T-01",
            CheckInAt = new DateTime(2026, 8, 11, 9, 0, 0, DateTimeKind.Utc)
        };
        _consultationRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(consultation);
        _patientRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Patient { PatientId = 1, Name = "Alice Ahmed", Phone = "01711111111" });
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Doctor { DoctorId = 10, FullName = "Dr. Ten" });
        _staffRepositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new Staff { StaffId = 3, FullName = "Amina Karim" });

        // Act
        var result = await _handler.Handle(new GetConsultationDetailsQuery(1), default);

        // Assert
        result.ConsultationId.Should().Be(1);
        result.DisplayCode.Should().Be("CN-2026-0001");
        result.PatientName.Should().Be("Alice Ahmed");
        result.PatientPhone.Should().Be("01711111111");
        result.DoctorName.Should().Be("Dr. Ten");
        result.RegisteredByName.Should().Be("Amina Karim");
    }

    [Fact]
    public async Task Handle_WhenConsultationDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _consultationRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Consultation?)null);

        // Act
        var act = () => _handler.Handle(new GetConsultationDetailsQuery(999), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
