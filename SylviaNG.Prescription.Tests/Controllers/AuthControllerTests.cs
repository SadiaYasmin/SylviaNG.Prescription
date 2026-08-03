using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SylviaNG.Prescription.Application.Features.Auth.Commands.CreateUserAccount;
using SylviaNG.Prescription.Application.Features.Auth.Commands.Login;
using SylviaNG.Prescription.Application.Features.Auth.Commands.Logout;
using SylviaNG.Prescription.Application.Features.Auth.Commands.RefreshToken;
using SylviaNG.Prescription.Application.Features.Auth.Commands.ResetPassword;
using SylviaNG.Prescription.Application.Features.Auth.Models;
using SylviaNG.Prescription.Controllers;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AuthController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Login_ShouldReturnOkWithLoginResponse()
    {
        // Arrange
        var request = new LoginRequest { Username = "doctor.dev", Password = "DevPassword123!" };
        var expected = new LoginResponse { AccessToken = "token", Username = "doctor.dev", Role = "Doctor" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), default))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Refresh_ShouldReturnOkWithRefreshTokenResponse()
    {
        // Arrange
        var request = new RefreshTokenRequest { RefreshToken = "refresh-1" };
        var expected = new RefreshTokenResponse { AccessToken = "new-token" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), default))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.Refresh(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Logout_ShouldReturnOk()
    {
        // Arrange
        var request = new LogoutRequest { RefreshToken = "refresh-1" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<LogoutCommand>(), default))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await _controller.Logout(request);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task CreateUserAccount_ShouldReturnOkWithCreatedAccount()
    {
        // Arrange
        var request = new CreateUserAccountRequest { Username = "new.doctor", Role = UserRoleEnum.Doctor };
        var expected = new CreateUserAccountResponse { UserId = 1, Username = "new.doctor", TemporaryPassword = "Temp123!" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateUserAccountCommand>(), default))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.CreateUserAccount(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnOkWithTemporaryPassword()
    {
        // Arrange
        var expected = new ResetPasswordResponse { TemporaryPassword = "Temp123!" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<ResetPasswordCommand>(), default))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.ResetPassword(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expected);
    }
}
