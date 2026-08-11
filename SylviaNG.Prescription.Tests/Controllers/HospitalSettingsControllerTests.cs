using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SylviaNG.Prescription.Application.Features.HospitalSettings.Commands.UpdateHospitalSettings;
using SylviaNG.Prescription.Application.Features.HospitalSettings.Models;
using SylviaNG.Prescription.Application.Features.HospitalSettings.Queries.GetHospitalSettings;
using SylviaNG.Prescription.Controllers;

namespace SylviaNG.Prescription.Tests.Controllers;

public class HospitalSettingsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly HospitalSettingsController _controller;

    public HospitalSettingsControllerTests()
    {
        _controller = new HospitalSettingsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Get_ShouldReturnOkWithHospitalSettings()
    {
        var expected = new HospitalSettingsResponse { HospitalSettingsId = 1, Name = "City Hospital" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetHospitalSettingsQuery>(), default)).ReturnsAsync(expected);

        var result = await _controller.Get();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Update_ShouldReturnOkWithUpdatedHospitalSettings()
    {
        var request = new UpdateHospitalSettingsRequest { Name = "New Name", Address = "New Address", Phone = "01700000000" };
        var expected = new HospitalSettingsResponse { HospitalSettingsId = 1, Name = "New Name" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateHospitalSettingsCommand>(), default)).ReturnsAsync(expected);

        var result = await _controller.Update(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }
}
