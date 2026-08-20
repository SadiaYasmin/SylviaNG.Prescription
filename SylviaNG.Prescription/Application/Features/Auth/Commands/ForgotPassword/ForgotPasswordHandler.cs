using MediatR;
using SylviaNG.Prescription.Application.Interfaces.Services;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, Unit>
    {
        private readonly IAuthService _authService;
        public ForgotPasswordHandler(IAuthService authService) => _authService = authService;

        public async Task<Unit> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
        {
            await _authService.RequestPasswordResetOtpAsync(command.Request.Email);
            return Unit.Value;
        }
    }
}
