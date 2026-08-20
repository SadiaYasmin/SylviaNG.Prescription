using MediatR;
using SylviaNG.Prescription.Application.Features.Auth.Models;
using SylviaNG.Prescription.Application.Interfaces.Services;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.VerifyForgotPasswordOtp
{
    public class VerifyForgotPasswordOtpHandler : IRequestHandler<VerifyForgotPasswordOtpCommand, VerifyForgotPasswordOtpResponse>
    {
        private readonly IAuthService _authService;
        public VerifyForgotPasswordOtpHandler(IAuthService authService) => _authService = authService;

        public async Task<VerifyForgotPasswordOtpResponse> Handle(VerifyForgotPasswordOtpCommand command, CancellationToken cancellationToken)
        {
            var valid = await _authService.VerifyPasswordResetOtpAsync(command.Request.Email, command.Request.Code);
            return new VerifyForgotPasswordOtpResponse { Valid = valid };
        }
    }
}
