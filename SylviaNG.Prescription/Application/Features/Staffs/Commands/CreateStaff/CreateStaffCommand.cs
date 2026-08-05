using MediatR;
using SylviaNG.Prescription.Application.Features.Staffs.Models;

namespace SylviaNG.Prescription.Application.Features.Staffs.Commands.CreateStaff
{
    public class CreateStaffCommand : IRequest<CreateStaffResponse>
    {
        public CreateStaffRequest Request { get; set; }

        public CreateStaffCommand(CreateStaffRequest request)
        {
            Request = request;
        }
    }
}
