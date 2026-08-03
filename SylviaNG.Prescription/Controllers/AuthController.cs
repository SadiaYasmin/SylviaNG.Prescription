using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SylviaNG.Prescription.Application.Features.Auth.Commands.CreateUserAccount;
using SylviaNG.Prescription.Application.Features.Auth.Commands.Login;
using SylviaNG.Prescription.Application.Features.Auth.Commands.Logout;
using SylviaNG.Prescription.Application.Features.Auth.Commands.RefreshToken;
using SylviaNG.Prescription.Application.Features.Auth.Commands.ResetPassword;
using SylviaNG.Prescription.Application.Features.Auth.Models;

namespace SylviaNG.Prescription.Controllers
{
    [ApiController]
    [Route("prescription/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Log in with username and password. Role is never accepted from the client —
        /// it comes from the authenticated account.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var result = await _mediator.Send(new LoginCommand(request));
            return Ok(result);
        }

        /// <summary>
        /// Exchange a refresh token for a new access token.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<RefreshTokenResponse>> Refresh([FromBody] RefreshTokenRequest request)
        {
            var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken));
            return Ok(result);
        }

        /// <summary>
        /// Log out, invalidating the session server-side (Keycloak token revocation).
        /// </summary>
        [HttpPost("logout")]
        public async Task<ActionResult> Logout([FromBody] LogoutRequest request)
        {
            await _mediator.Send(new LogoutCommand(request.RefreshToken));
            return Ok();
        }

        /// <summary>
        /// Admin-only: create a new Doctor/Staff/Admin account.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("users")]
        public async Task<ActionResult<CreateUserAccountResponse>> CreateUserAccount([FromBody] CreateUserAccountRequest request)
        {
            var result = await _mediator.Send(new CreateUserAccountCommand(request));
            return Ok(result);
        }

        /// <summary>
        /// Admin-only: force a temporary password reset for an existing account.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("users/{userId}/reset-password")]
        public async Task<ActionResult<ResetPasswordResponse>> ResetPassword(long userId)
        {
            var result = await _mediator.Send(new ResetPasswordCommand(userId));
            return Ok(result);
        }
    }
}
