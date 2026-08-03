using MediatR;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.Logout
{
    public class LogoutCommand : IRequest<Unit>
    {
        public string RefreshToken { get; set; }

        public LogoutCommand(string refreshToken)
        {
            RefreshToken = refreshToken;
        }
    }
}
