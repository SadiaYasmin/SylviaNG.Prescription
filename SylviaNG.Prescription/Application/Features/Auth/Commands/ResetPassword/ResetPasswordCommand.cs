using MediatR;
using SylviaNG.Prescription.Application.Features.Auth.Models;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommand : IRequest<ResetPasswordResponse>
    {
        public long UserId { get; set; }

        public ResetPasswordCommand(long userId)
        {
            UserId = userId;
        }
    }
}
