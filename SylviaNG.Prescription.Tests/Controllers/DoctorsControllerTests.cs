using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SylviaNG.Prescription.Application.Features.Doctors.Commands.CreateDoctor;
using SylviaNG.Prescription.Application.Features.Doctors.Commands.DeactivateDoctor;
using SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctor;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Features.Doctors.Queries.GetDoctorDetails;
using SylviaNG.Prescription.Application.Features.Doctors.Queries.GetDoctorList;
using SylviaNG.Prescription.Controllers;

namespace SylviaNG.Prescription.Tests.Controllers;

public class DoctorsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly DoctorsController _controller;

    public DoctorsControllerTests()
    {
        _controller = new DoctorsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Create_ShouldReturnOkWithCreatedDoctor()
    {
        var request = new CreateDoctorRequest { Username = "new.doctor", FullName = "Dr. Jane Doe", Phone = "01712345678" };
        var expected = new CreateDoctorResponse { DoctorId = 1, UserId = 1, Username = "new.doctor", TemporaryPassword = "Temp123!" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateDoctorCommand>(), default)).ReturnsAsync(expected);

        var result = await _controller.Create(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Update_ShouldReturnOkWithUpdatedDoctor()
    {
        var request = new UpdateDoctorRequest { FullName = "Dr. Jane Doe", Phone = "01712345678" };
        var expected = new DoctorSummaryResponse { DoctorId = 1, FullName = "Dr. Jane Doe" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateDoctorCommand>(), default)).ReturnsAsync(expected);

        var result = await _controller.Update(1, request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Deactivate_ShouldReturnOk()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeactivateDoctorCommand>(), default)).ReturnsAsync(Unit.Value);

        var result = await _controller.Deactivate(1);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetList_ShouldReturnOkWithDoctorList()
    {
        var expected = new DoctorListResponse { TotalCount = 0 };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDoctorListQuery>(), default)).ReturnsAsync(expected);

        var result = await _controller.GetList(new DoctorListRequest());

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetDetails_ShouldReturnOkWithDoctorDetails()
    {
        var expected = new DoctorDetailsResponse();
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDoctorDetailsQuery>(), default)).ReturnsAsync(expected);

        var result = await _controller.GetDetails(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }
}
