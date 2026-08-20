using MediatR;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Auth.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;

namespace SylviaNG.Prescription.Application.Features.Auth.Queries.GetCurrentUser
{
    public class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse>
    {
        private readonly IUserRepository _userRepository;
        public GetCurrentUserHandler(IUserRepository userRepository) => _userRepository = userRepository;

        public async Task<CurrentUserResponse> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByKeycloakIdAsync(query.KeycloakId)
                ?? throw new NotFoundException("User", query.KeycloakId);

            return new CurrentUserResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString()
            };
        }
    }
}
