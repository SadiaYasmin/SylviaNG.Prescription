using MediatR;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommand : IRequest<Unit>
    {
        public long UserId { get; set; }

        public ResetPasswordCommand(long userId)
        {
            UserId = userId;
        }
    }
}
