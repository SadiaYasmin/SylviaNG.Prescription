using MediatR;
using SylviaNG.Prescription.Application.Interfaces.Services;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.RequestEmailChange
{
    public class RequestEmailChangeHandler : IRequestHandler<RequestEmailChangeCommand, Unit>
    {
        private readonly IAuthService _authService;
        public RequestEmailChangeHandler(IAuthService authService) => _authService = authService;

        public async Task<Unit> Handle(RequestEmailChangeCommand command, CancellationToken cancellationToken)
        {
            await _authService.RequestEmailChangeAsync(command.KeycloakId, command.Request.NewEmail);
            return Unit.Value;
        }
    }
}
