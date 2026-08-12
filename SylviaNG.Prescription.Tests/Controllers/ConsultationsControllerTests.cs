using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SylviaNG.Prescription.Application.Features.Consultations.Commands.CreateConsultation;
using SylviaNG.Prescription.Application.Features.Consultations.Commands.OpenConsultation;
using SylviaNG.Prescription.Application.Features.Consultations.Models;
using SylviaNG.Prescription.Application.Features.Consultations.Queries.GetConsultationDetails;
using SylviaNG.Prescription.Application.Features.Consultations.Queries.GetConsultationList;
using SylviaNG.Prescription.Application.Features.Consultations.Queries.GetMyAssignedDoctors;
using SylviaNG.Prescription.Application.Features.Consultations.Queries.GetMyQueue;
using SylviaNG.Prescription.Application.Features.Consultations.Queries.GetTodaysQueue;
using SylviaNG.Prescription.Controllers;

namespace SylviaNG.Prescription.Tests.Controllers;

public class ConsultationsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly ConsultationsController _controller;

    public ConsultationsControllerTests()
    {
        _controller = new ConsultationsController(_mediatorMock.Object)
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
        var request = new CreateConsultationRequest { PatientId = 1, DoctorId = 10 };
        var expected = new CreateConsultationResponse { DuplicateFound = false };
        CreateConsultationCommand? sentCommand = null;
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateConsultationCommand>(), default))
            .Callback<IRequest<CreateConsultationResponse>, CancellationToken>((c, _) => sentCommand = (CreateConsultationCommand)c)
            .ReturnsAsync(expected);

        var result = await _controller.Create(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
        sentCommand.Should().NotBeNull();
        sentCommand!.KeycloakId.Should().Be("kc-123");
        sentCommand.Request.Should().Be(request);
    }

    [Fact]
    public async Task Open_ShouldSendCommandBuiltFromRouteIdAndCallerClaim_AndReturnOk()
    {
        var expected = new OpenConsultationResponse { ConsultationId = 1 };
        OpenConsultationCommand? sentCommand = null;
        _mediatorMock.Setup(m => m.Send(It.IsAny<OpenConsultationCommand>(), default))
            .Callback<IRequest<OpenConsultationResponse>, CancellationToken>((c, _) => sentCommand = (OpenConsultationCommand)c)
            .ReturnsAsync(expected);

        var result = await _controller.Open(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
        sentCommand.Should().NotBeNull();
        sentCommand!.ConsultationId.Should().Be(1);
        sentCommand.KeycloakId.Should().Be("kc-123");
    }

    [Fact]
    public async Task GetTodaysQueue_ShouldSendQueryBuiltFromCallerClaim_AndReturnOk()
    {
        var expected = new List<QueueItemResponse>();
        GetTodaysQueueQuery? sentQuery = null;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetTodaysQueueQuery>(), default))
            .Callback<IRequest<List<QueueItemResponse>>, CancellationToken>((q, _) => sentQuery = (GetTodaysQueueQuery)q)
            .ReturnsAsync(expected);

        var result = await _controller.GetTodaysQueue();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
        sentQuery.Should().NotBeNull();
        sentQuery!.KeycloakId.Should().Be("kc-123");
    }

    [Fact]
    public async Task GetMyQueue_ShouldSendQueryBuiltFromCallerClaim_AndReturnOk()
    {
        var expected = new List<QueueItemResponse>();
        GetMyQueueQuery? sentQuery = null;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetMyQueueQuery>(), default))
            .Callback<IRequest<List<QueueItemResponse>>, CancellationToken>((q, _) => sentQuery = (GetMyQueueQuery)q)
            .ReturnsAsync(expected);

        var result = await _controller.GetMyQueue();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
        sentQuery.Should().NotBeNull();
        sentQuery!.KeycloakId.Should().Be("kc-123");
    }

    [Fact]
    public async Task GetMyAssignedDoctors_ShouldSendQueryBuiltFromCallerClaim_AndReturnOk()
    {
        var expected = new List<AssignedDoctorSummaryResponse>();
        GetMyAssignedDoctorsQuery? sentQuery = null;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetMyAssignedDoctorsQuery>(), default))
            .Callback<IRequest<List<AssignedDoctorSummaryResponse>>, CancellationToken>((q, _) => sentQuery = (GetMyAssignedDoctorsQuery)q)
            .ReturnsAsync(expected);

        var result = await _controller.GetMyAssignedDoctors();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
        sentQuery.Should().NotBeNull();
        sentQuery!.KeycloakId.Should().Be("kc-123");
    }

    [Fact]
    public async Task GetList_ShouldSendQueryBuiltFromRequest_AndReturnOk()
    {
        var expected = new ConsultationListResponse { TotalCount = 0 };
        GetConsultationListQuery? sentQuery = null;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetConsultationListQuery>(), default))
            .Callback<IRequest<ConsultationListResponse>, CancellationToken>((q, _) => sentQuery = (GetConsultationListQuery)q)
            .ReturnsAsync(expected);

        var request = new ConsultationListRequest { DoctorId = 10 };
        var result = await _controller.GetList(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
        sentQuery.Should().NotBeNull();
        sentQuery!.Request.Should().Be(request);
    }

    [Fact]
    public async Task GetDetails_ShouldSendQueryBuiltFromRouteId_AndReturnOk()
    {
        var expected = new ConsultationDetailsResponse { ConsultationId = 1 };
        GetConsultationDetailsQuery? sentQuery = null;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetConsultationDetailsQuery>(), default))
            .Callback<IRequest<ConsultationDetailsResponse>, CancellationToken>((q, _) => sentQuery = (GetConsultationDetailsQuery)q)
            .ReturnsAsync(expected);

        var result = await _controller.GetDetails(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
        sentQuery.Should().NotBeNull();
        sentQuery!.ConsultationId.Should().Be(1);
    }
}
