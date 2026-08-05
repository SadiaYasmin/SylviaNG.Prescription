using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SylviaNG.Prescription.Application.Features.Staffs.Commands.CreateStaff;
using SylviaNG.Prescription.Application.Features.Staffs.Commands.DeactivateStaff;
using SylviaNG.Prescription.Application.Features.Staffs.Commands.UpdateStaff;
using SylviaNG.Prescription.Application.Features.Staffs.Models;
using SylviaNG.Prescription.Application.Features.Staffs.Queries.GetAssignedStaff;
using SylviaNG.Prescription.Application.Features.Staffs.Queries.GetStaffDetails;
using SylviaNG.Prescription.Application.Features.Staffs.Queries.GetStaffList;
using SylviaNG.Prescription.Controllers;

namespace SylviaNG.Prescription.Tests.Controllers;

public class StaffControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly StaffController _controller;

    public StaffControllerTests()
    {
        _controller = new StaffController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Create_ShouldReturnOkWithCreatedStaff()
    {
        var request = new CreateStaffRequest { Username = "new.staff", FullName = "Jane Roy", Phone = "01712345678" };
        var expected = new CreateStaffResponse { StaffId = 1, UserId = 1, Username = "new.staff", TemporaryPassword = "Temp123!" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateStaffCommand>(), default)).ReturnsAsync(expected);

        var result = await _controller.Create(request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Update_ShouldReturnOkWithUpdatedStaff()
    {
        var request = new UpdateStaffRequest { FullName = "Jane Roy", Phone = "01712345678" };
        var expected = new StaffSummaryResponse { StaffId = 1, FullName = "Jane Roy" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateStaffCommand>(), default)).ReturnsAsync(expected);

        var result = await _controller.Update(1, request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Deactivate_ShouldReturnOk()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeactivateStaffCommand>(), default)).ReturnsAsync(Unit.Value);

        var result = await _controller.Deactivate(1);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetList_ShouldReturnOkWithStaffList()
    {
        var expected = new StaffListResponse { TotalCount = 0 };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetStaffListQuery>(), default)).ReturnsAsync(expected);

        var result = await _controller.GetList(new StaffListRequest());

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetDetails_ShouldReturnOkWithStaffDetails()
    {
        var expected = new StaffDetailsResponse();
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetStaffDetailsQuery>(), default)).ReturnsAsync(expected);

        var result = await _controller.GetDetails(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetAssignedToMe_ShouldSendQueryBuiltFromCallerClaim()
    {
        var expected = new StaffListResponse { TotalCount = 0 };
        GetAssignedStaffQuery? sentQuery = null;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAssignedStaffQuery>(), default))
            .Callback<IRequest<StaffListResponse>, CancellationToken>((q, _) => sentQuery = (GetAssignedStaffQuery)q)
            .ReturnsAsync(expected);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "kc-123") }))
            }
        };

        var result = await _controller.GetAssignedToMe("front desk");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
        sentQuery.Should().NotBeNull();
        sentQuery!.KeycloakId.Should().Be("kc-123");
        sentQuery.SearchTerm.Should().Be("front desk");
    }
}
