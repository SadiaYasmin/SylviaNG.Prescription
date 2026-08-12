using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SylviaNG.Prescription.Application.Features.Patients.Commands.CreatePatient;
using SylviaNG.Prescription.Application.Features.Patients.Commands.UpdatePatient;
using SylviaNG.Prescription.Application.Features.Patients.Models;
using SylviaNG.Prescription.Application.Features.Patients.Queries.GetPatientDetails;
using SylviaNG.Prescription.Application.Features.Patients.Queries.GetPatientList;
using SylviaNG.Prescription.Controllers;

namespace SylviaNG.Prescription.Tests.Controllers;

public class PatientsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly PatientsController _controller;

    public PatientsControllerTests()
    {
        _controller = new PatientsController(_mediatorMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "kc-123") }))
                }
            }
        };
    }

    [Fact]
    public async Task Create_ShouldSendCommandBuiltFromCallerClaim_AndReturnOk()
    {
        var request = new CreatePatientRequest { Name = "John Doe", Phone = "01712345678" };
        var expected = new PatientSummaryResponse { PatientId = 1, Name = "John Doe" };
        CreatePatientCommand? sentCommand = null;
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreatePatientCommand>(), default))
            .Callback<IRequest<PatientSummaryResponse>, CancellationToken>((c, _) => sentCommand = (CreatePatientCommand)c)
            .ReturnsAsync(expected);

        var result = await _controller.Create(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
        sentCommand.Should().NotBeNull();
        sentCommand!.KeycloakId.Should().Be("kc-123");
        sentCommand.Request.Should().Be(request);
    }

    [Fact]
    public async Task Update_ShouldSendCommandBuiltFromCallerClaim_AndReturnOk()
    {
        var request = new UpdatePatientRequest { Name = "John Doe", Phone = "01712345678" };
        var expected = new PatientSummaryResponse { PatientId = 1, Name = "John Doe" };
        UpdatePatientCommand? sentCommand = null;
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdatePatientCommand>(), default))
            .Callback<IRequest<PatientSummaryResponse>, CancellationToken>((c, _) => sentCommand = (UpdatePatientCommand)c)
            .ReturnsAsync(expected);

        var result = await _controller.Update(1, request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
        sentCommand.Should().NotBeNull();
        sentCommand!.PatientId.Should().Be(1);
        sentCommand.KeycloakId.Should().Be("kc-123");
    }

    [Fact]
    public async Task GetList_ShouldSendQueryBuiltFromCallerClaim_AndReturnOk()
    {
        var expected = new PatientListResponse { TotalCount = 0 };
        GetPatientListQuery? sentQuery = null;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPatientListQuery>(), default))
            .Callback<IRequest<PatientListResponse>, CancellationToken>((q, _) => sentQuery = (GetPatientListQuery)q)
            .ReturnsAsync(expected);

        var result = await _controller.GetList(new PatientListRequest { SearchTerm = "john" });

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
        sentQuery.Should().NotBeNull();
        sentQuery!.KeycloakId.Should().Be("kc-123");
        sentQuery.Request.SearchTerm.Should().Be("john");
    }

    [Fact]
    public async Task GetDetails_ShouldSendQueryBuiltFromCallerClaim_AndReturnOk()
    {
        var expected = new PatientDetailsResponse();
        GetPatientDetailsQuery? sentQuery = null;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPatientDetailsQuery>(), default))
            .Callback<IRequest<PatientDetailsResponse>, CancellationToken>((q, _) => sentQuery = (GetPatientDetailsQuery)q)
            .ReturnsAsync(expected);

        var result = await _controller.GetDetails(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
        sentQuery.Should().NotBeNull();
        sentQuery!.PatientId.Should().Be(1);
        sentQuery.KeycloakId.Should().Be("kc-123");
    }
}
