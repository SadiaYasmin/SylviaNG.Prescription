using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SylviaNG.Prescription.Application.Features.Auth.Commands.ConfirmEmailChange;
using SylviaNG.Prescription.Application.Features.Auth.Commands.ConfirmPasswordChange;
using SylviaNG.Prescription.Application.Features.Auth.Commands.CreateUserAccount;
using SylviaNG.Prescription.Application.Features.Auth.Commands.ForgotPassword;
using SylviaNG.Prescription.Application.Features.Auth.Commands.Login;
using SylviaNG.Prescription.Application.Features.Auth.Commands.Logout;
using SylviaNG.Prescription.Application.Features.Auth.Commands.RefreshToken;
using SylviaNG.Prescription.Application.Features.Auth.Commands.RequestEmailChange;
using SylviaNG.Prescription.Application.Features.Auth.Commands.RequestPasswordChange;
using SylviaNG.Prescription.Application.Features.Auth.Commands.ResetPassword;
using SylviaNG.Prescription.Application.Features.Auth.Commands.ResetPasswordWithOtp;
using SylviaNG.Prescription.Application.Features.Auth.Commands.VerifyForgotPasswordOtp;
using SylviaNG.Prescription.Application.Features.Auth.Models;
using SylviaNG.Prescription.Application.Features.Auth.Queries.GetCurrentUser;

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
        /// Admin-only: create a new Doctor/Staff/Admin account. No password is issued here —
        /// the invitee gets an email with a link to set their own.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("users")]
        public async Task<ActionResult<CreateUserAccountResponse>> CreateUserAccount([FromBody] CreateUserAccountRequest request)
        {
            var result = await _mediator.Send(new CreateUserAccountCommand(request));
            return Ok(result);
        }

        /// <summary>
        /// Admin-only: re-send the "set your password" invite email for an existing account.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("users/{userId}/reset-password")]
        public async Task<ActionResult> ResetPassword(long userId)
        {
            await _mediator.Send(new ResetPasswordCommand(userId));
            return Ok();
        }

        // ===================== Forgot password (anonymous, OTP) =====================

        /// <summary>
        /// Always responds the same way whether or not the email is registered — the
        /// response never discloses which emails have accounts.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            await _mediator.Send(new ForgotPasswordCommand(request));
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("forgot-password/verify")]
        public async Task<ActionResult<VerifyForgotPasswordOtpResponse>> VerifyForgotPasswordOtp([FromBody] VerifyForgotPasswordOtpRequest request)
        {
            var result = await _mediator.Send(new VerifyForgotPasswordOtpCommand(request));
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("forgot-password/reset")]
        public async Task<ActionResult> ResetPasswordWithOtp([FromBody] ResetPasswordWithOtpRequest request)
        {
            await _mediator.Send(new ResetPasswordWithOtpCommand(request));
            return Ok();
        }

        // ===================== Self-service change email / password =====================

        /// <summary>Any authenticated role — the shared "My Profile" page's identity header.</summary>
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<CurrentUserResponse>> GetCurrentUser()
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new GetCurrentUserQuery(keycloakId));
            return Ok(result);
        }

        [Authorize]
        [HttpPost("me/email/request-change")]
        public async Task<ActionResult> RequestEmailChange([FromBody] RequestEmailChangeRequest request)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _mediator.Send(new RequestEmailChangeCommand(keycloakId, request));
            return Ok();
        }

        [Authorize]
        [HttpPost("me/email/confirm-change")]
        public async Task<ActionResult> ConfirmEmailChange([FromBody] ConfirmEmailChangeRequest request)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _mediator.Send(new ConfirmEmailChangeCommand(keycloakId, request));
            return Ok();
        }

        [Authorize]
        [HttpPost("me/password/request-change")]
        public async Task<ActionResult> RequestPasswordChange()
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _mediator.Send(new RequestPasswordChangeCommand(keycloakId));
            return Ok();
        }

        [Authorize]
        [HttpPost("me/password/confirm-change")]
        public async Task<ActionResult> ConfirmPasswordChange([FromBody] ConfirmPasswordChangeRequest request)
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _mediator.Send(new ConfirmPasswordChangeCommand(keycloakId, request));
            return Ok();
        }
    }
}
