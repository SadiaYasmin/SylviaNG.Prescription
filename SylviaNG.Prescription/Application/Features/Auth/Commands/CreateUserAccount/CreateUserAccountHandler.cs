using MediatR;
using SylviaNG.Prescription.Application.Features.Auth.Models;
using SylviaNG.Prescription.Application.Interfaces.Services;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.CreateUserAccount
{
    public class CreateUserAccountHandler : IRequestHandler<CreateUserAccountCommand, CreateUserAccountResponse>
    {
        private readonly IAuthService _authService;

        public CreateUserAccountHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<CreateUserAccountResponse> Handle(CreateUserAccountCommand command, CancellationToken cancellationToken)
        {
            return await _authService.CreateUserAccountAsync(command.Request);
        }
    }
}
