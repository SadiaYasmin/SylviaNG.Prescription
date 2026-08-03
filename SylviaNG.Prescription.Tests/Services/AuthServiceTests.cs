using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Common.Models;
using SylviaNG.Prescription.Application.Features.Auth.Models;
using SylviaNG.Prescription.Application.Interfaces.Externals;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Services;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;
using System.Text;
using System.Text.Json;

namespace SylviaNG.Prescription.Tests.Services;

public class AuthServiceTests
{
    private const string RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IKeycloakTokenClient> _tokenClientMock;
    private readonly Mock<IKeycloakAdminClient> _adminClientMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tokenClientMock = new Mock<IKeycloakTokenClient>();
        _adminClientMock = new Mock<IKeycloakAdminClient>();
        _service = new AuthService(_userRepositoryMock.Object, _unitOfWorkMock.Object, _tokenClientMock.Object, _adminClientMock.Object);
    }

    private static string BuildFakeAccessToken(string sub, string role)
    {
        var header = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"alg\":\"none\"}")).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var payload = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["sub"] = sub,
            [RoleClaimType] = new[] { role }
        });
        var encodedPayload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return $"{header}.{encodedPayload}.fake-signature";
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_AndNoLocalRecordYet_ShouldProvisionUserAndReturnLoginResponse()
    {
        // Arrange
        var request = new LoginRequest { Username = "doctor.dev", Password = "DevPassword123!" };
        var accessToken = BuildFakeAccessToken("kc-123", "Doctor");

        _tokenClientMock.Setup(t => t.PasswordGrantAsync(request.Username, request.Password))
            .ReturnsAsync(new KeycloakTokenResult { AccessToken = accessToken, RefreshToken = "refresh-1", ExpiresIn = 300 });

        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-123"))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.AccessToken.Should().Be(accessToken);
        result.Role.Should().Be("Doctor");
        result.Username.Should().Be("doctor.dev");
        _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u => u.KeycloakId == "kc-123" && u.Role == UserRoleEnum.Doctor)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithExistingLocalRecord_ShouldNotProvisionAgain()
    {
        // Arrange
        var request = new LoginRequest { Username = "doctor.dev", Password = "DevPassword123!" };
        var accessToken = BuildFakeAccessToken("kc-123", "Doctor");
        var existing = new User { UserId = 5, KeycloakId = "kc-123", Username = "doctor.dev", Role = UserRoleEnum.Doctor, IsActive = true };

        _tokenClientMock.Setup(t => t.PasswordGrantAsync(request.Username, request.Password))
            .ReturnsAsync(new KeycloakTokenResult { AccessToken = accessToken, RefreshToken = "refresh-1", ExpiresIn = 300 });

        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-123"))
            .ReturnsAsync(existing);

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        result.Username.Should().Be("doctor.dev");
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest { Username = "doctor.dev", Password = "wrong" };
        _tokenClientMock.Setup(t => t.PasswordGrantAsync(request.Username, request.Password))
            .ReturnsAsync((KeycloakTokenResult?)null);

        // Act
        var act = () => _service.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task LoginAsync_WithInactiveLocalUser_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest { Username = "doctor.dev", Password = "DevPassword123!" };
        var accessToken = BuildFakeAccessToken("kc-123", "Doctor");
        var existing = new User { UserId = 5, KeycloakId = "kc-123", Username = "doctor.dev", Role = UserRoleEnum.Doctor, IsActive = false };

        _tokenClientMock.Setup(t => t.PasswordGrantAsync(request.Username, request.Password))
            .ReturnsAsync(new KeycloakTokenResult { AccessToken = accessToken, RefreshToken = "refresh-1", ExpiresIn = 300 });
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-123"))
            .ReturnsAsync(existing);

        // Act
        var act = () => _service.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task RefreshAsync_WithValidRefreshToken_ShouldReturnNewTokens()
    {
        // Arrange
        _tokenClientMock.Setup(t => t.RefreshAsync("refresh-1"))
            .ReturnsAsync(new KeycloakTokenResult { AccessToken = "new-access", RefreshToken = "new-refresh", ExpiresIn = 300 });

        // Act
        var result = await _service.RefreshAsync("refresh-1");

        // Assert
        result.AccessToken.Should().Be("new-access");
    }

    [Fact]
    public async Task RefreshAsync_WithInvalidRefreshToken_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        _tokenClientMock.Setup(t => t.RefreshAsync("bad-token"))
            .ReturnsAsync((KeycloakTokenResult?)null);

        // Act
        var act = () => _service.RefreshAsync("bad-token");

        // Assert
        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task LogoutAsync_ShouldCallKeycloakLogout()
    {
        // Act
        await _service.LogoutAsync("refresh-1");

        // Assert
        _tokenClientMock.Verify(t => t.LogoutAsync("refresh-1"), Times.Once);
    }

    [Fact]
    public async Task CreateUserAccountAsync_WithNewUsername_ShouldCreateKeycloakUserAndLocalRecord()
    {
        // Arrange
        var request = new CreateUserAccountRequest { Username = "new.doctor", Email = "new.doctor@example.com", Role = UserRoleEnum.Doctor };

        _userRepositoryMock.Setup(r => r.ExistsByUsernameAsync(request.Username)).ReturnsAsync(false);
        _adminClientMock.Setup(a => a.CreateUserAsync(request.Username, request.Email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "Doctor"))
            .ReturnsAsync(new KeycloakCreatedUser { KeycloakId = "kc-999" });
        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => u.UserId = 42);

        // Act
        var result = await _service.CreateUserAccountAsync(request);

        // Assert
        result.UserId.Should().Be(42);
        result.TemporaryPassword.Should().NotBeNullOrEmpty();
        _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u => u.KeycloakId == "kc-999")), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateUserAccountAsync_WithDuplicateUsername_ShouldThrowDuplicateException_WithoutCallingKeycloak()
    {
        // Arrange
        var request = new CreateUserAccountRequest { Username = "existing.doctor", Role = UserRoleEnum.Doctor };
        _userRepositoryMock.Setup(r => r.ExistsByUsernameAsync(request.Username)).ReturnsAsync(true);

        // Act
        var act = () => _service.CreateUserAccountAsync(request);

        // Assert
        await act.Should().ThrowAsync<DuplicateException>();
        _adminClientMock.Verify(a => a.CreateUserAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithExistingUser_ShouldSetTemporaryPasswordInKeycloak()
    {
        // Arrange
        var user = new User { UserId = 7, KeycloakId = "kc-777", Username = "doctor.dev", Role = UserRoleEnum.Doctor };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(user);

        // Act
        var result = await _service.ResetPasswordAsync(7);

        // Assert
        result.TemporaryPassword.Should().NotBeNullOrEmpty();
        _adminClientMock.Verify(a => a.SetTemporaryPasswordAsync("kc-777", result.TemporaryPassword), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithNonExistentUser_ShouldThrowNotFoundException()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);

        // Act
        var act = () => _service.ResetPasswordAsync(999);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
