using MediatR;
using SylviaNG.Prescription.Application.Features.Auth.Models;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.ResetPasswordWithOtp
{
    public class ResetPasswordWithOtpCommand : IRequest<Unit>
    {
        public ResetPasswordWithOtpRequest Request { get; set; }
        public ResetPasswordWithOtpCommand(ResetPasswordWithOtpRequest request) => Request = request;
    }
}
