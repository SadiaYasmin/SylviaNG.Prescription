using MediatR;
using SylviaNG.Prescription.Application.Interfaces.Services;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.Logout
{
    public class LogoutHandler : IRequestHandler<LogoutCommand, Unit>
    {
        private readonly IAuthService _authService;

        public LogoutHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<Unit> Handle(LogoutCommand command, CancellationToken cancellationToken)
        {
            await _authService.LogoutAsync(command.RefreshToken);
            return Unit.Value;
        }
    }
}
