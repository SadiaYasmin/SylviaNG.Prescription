using MediatR;
using SylviaNG.Prescription.Application.Features.Auth.Models;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.VerifyForgotPasswordOtp
{
    public class VerifyForgotPasswordOtpCommand : IRequest<VerifyForgotPasswordOtpResponse>
    {
        public VerifyForgotPasswordOtpRequest Request { get; set; }
        public VerifyForgotPasswordOtpCommand(VerifyForgotPasswordOtpRequest request) => Request = request;
    }
}
