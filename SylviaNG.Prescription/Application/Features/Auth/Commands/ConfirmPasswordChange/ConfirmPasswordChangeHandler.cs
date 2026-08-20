using MediatR;
using SylviaNG.Prescription.Application.Interfaces.Services;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.ConfirmPasswordChange
{
    public class ConfirmPasswordChangeHandler : IRequestHandler<ConfirmPasswordChangeCommand, Unit>
    {
        private readonly IAuthService _authService;
        public ConfirmPasswordChangeHandler(IAuthService authService) => _authService = authService;

        public async Task<Unit> Handle(ConfirmPasswordChangeCommand command, CancellationToken cancellationToken)
        {
            await _authService.ConfirmPasswordChangeAsync(command.KeycloakId, command.Request.Code, command.Request.NewPassword);
            return Unit.Value;
        }
    }
}
