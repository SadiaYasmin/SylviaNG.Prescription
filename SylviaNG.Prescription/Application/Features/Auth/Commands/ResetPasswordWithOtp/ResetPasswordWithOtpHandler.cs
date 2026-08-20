using MediatR;
using SylviaNG.Prescription.Application.Interfaces.Services;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.ResetPasswordWithOtp
{
    public class ResetPasswordWithOtpHandler : IRequestHandler<ResetPasswordWithOtpCommand, Unit>
    {
        private readonly IAuthService _authService;
        public ResetPasswordWithOtpHandler(IAuthService authService) => _authService = authService;

        public async Task<Unit> Handle(ResetPasswordWithOtpCommand command, CancellationToken cancellationToken)
        {
            await _authService.ResetPasswordWithOtpAsync(command.Request.Email, command.Request.Code, command.Request.NewPassword);
            return Unit.Value;
        }
    }
}
