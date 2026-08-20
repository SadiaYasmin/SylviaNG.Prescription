using MediatR;
using SylviaNG.Prescription.Application.Interfaces.Services;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.ConfirmEmailChange
{
    public class ConfirmEmailChangeHandler : IRequestHandler<ConfirmEmailChangeCommand, Unit>
    {
        private readonly IAuthService _authService;
        public ConfirmEmailChangeHandler(IAuthService authService) => _authService = authService;

        public async Task<Unit> Handle(ConfirmEmailChangeCommand command, CancellationToken cancellationToken)
        {
            await _authService.ConfirmEmailChangeAsync(command.KeycloakId, command.Request.Code);
            return Unit.Value;
        }
    }
}
