using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SylviaNG.Prescription.Application.Features.JobPostings.Commands.JobPostingCreate;
using SylviaNG.Prescription.Application.Features.JobPostings.Commands.JobPostingDelete;
using SylviaNG.Prescription.Application.Features.JobPostings.Models;
using SylviaNG.Prescription.Application.Features.JobPostings.Queries.JobPostingGetAll;
using SylviaNG.Prescription.Application.Features.JobPostings.Queries.JobPostingGetById;
using SylviaNG.Prescription.Controllers;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Tests.Controllers;

public class JobPostingControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly JobPostingController _controller;

    public JobPostingControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new JobPostingController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithJobPostings()
    {
        // Arrange
        var expected = new List<JobPostingResponse>
        {
            new() { JobPostingId = 1, Title = "Software Engineer", Status = JobStatusEnum.Open },
            new() { JobPostingId = 2, Title = "UI Designer", Status = JobStatusEnum.Draft }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<JobPostingGetAllQuery>(), default))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetById_ShouldReturnOkWithJobPosting()
    {
        // Arrange
        var expected = new JobPostingResponse { JobPostingId = 1, Title = "Software Engineer" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<JobPostingGetByIdQuery>(), default))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Create_WithValidRequest_ShouldReturnCreatedId()
    {
        // Arrange
        var request = new JobPostingCreateRequest
        {
            Title = "New Job",
            SiteId = 1
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<JobPostingCreateCommand>(), default))
            .ReturnsAsync(1L);

        // Act
        var result = await _controller.Create(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(1L);
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnOk()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<JobPostingDeleteCommand>(), default))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }
}
