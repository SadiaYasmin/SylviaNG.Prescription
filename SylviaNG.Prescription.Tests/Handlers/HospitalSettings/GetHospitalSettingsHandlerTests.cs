using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.HospitalSettings.Queries.GetHospitalSettings;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Tests.Handlers.HospitalSettings;

public class GetHospitalSettingsHandlerTests
{
    private readonly Mock<IHospitalSettingsRepository> _hospitalSettingsRepositoryMock = new();
    private readonly GetHospitalSettingsHandler _handler;

    public GetHospitalSettingsHandlerTests()
    {
        _handler = new GetHospitalSettingsHandler(_hospitalSettingsRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingRow_ShouldReturnIt()
    {
        // Arrange
        var settings = new List<Domain.Entities.HospitalSettings>
        {
            new() { HospitalSettingsId = 1, Name = "City Hospital", Address = "123 Main St", Phone = "01700000000" }
        };
        _hospitalSettingsRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(settings.BuildMock());

        // Act
        var result = await _handler.Handle(new GetHospitalSettingsQuery(), default);

        // Assert
        result.Name.Should().Be("City Hospital");
        result.Phone.Should().Be("01700000000");
    }

    [Fact]
    public async Task Handle_WithNoRow_ShouldThrowNotFoundException()
    {
        // Arrange: deliberately dumb — does not create-on-read (the startup seeder guarantees
        // the row exists in practice).
        var settings = new List<Domain.Entities.HospitalSettings>();
        _hospitalSettingsRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(settings.BuildMock());

        // Act
        var act = () => _handler.Handle(new GetHospitalSettingsQuery(), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
