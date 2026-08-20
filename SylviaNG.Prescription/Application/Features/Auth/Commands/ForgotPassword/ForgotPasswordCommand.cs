using MediatR;
using SylviaNG.Prescription.Application.Features.Auth.Models;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommand : IRequest<Unit>
    {
        public ForgotPasswordRequest Request { get; set; }
        public ForgotPasswordCommand(ForgotPasswordRequest request) => Request = request;
    }
}
