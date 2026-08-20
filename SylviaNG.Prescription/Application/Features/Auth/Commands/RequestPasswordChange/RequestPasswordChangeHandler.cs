using MediatR;
using SylviaNG.Prescription.Application.Interfaces.Services;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.RequestPasswordChange
{
    public class RequestPasswordChangeHandler : IRequestHandler<RequestPasswordChangeCommand, Unit>
    {
        private readonly IAuthService _authService;
        public RequestPasswordChangeHandler(IAuthService authService) => _authService = authService;

        public async Task<Unit> Handle(RequestPasswordChangeCommand command, CancellationToken cancellationToken)
        {
            await _authService.RequestPasswordChangeAsync(command.KeycloakId);
            return Unit.Value;
        }
    }
}
