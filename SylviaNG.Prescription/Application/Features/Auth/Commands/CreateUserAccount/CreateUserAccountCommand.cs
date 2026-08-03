using MediatR;
using SylviaNG.Prescription.Application.Features.Auth.Models;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.CreateUserAccount
{
    public class CreateUserAccountCommand : IRequest<CreateUserAccountResponse>
    {
        public CreateUserAccountRequest Request { get; set; }

        public CreateUserAccountCommand(CreateUserAccountRequest request)
        {
            Request = request;
        }
    }
}
