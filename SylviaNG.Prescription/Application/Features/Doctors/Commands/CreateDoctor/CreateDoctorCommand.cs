using MediatR;
using SylviaNG.Prescription.Application.Features.Doctors.Models;

namespace SylviaNG.Prescription.Application.Features.Doctors.Commands.CreateDoctor
{
    public class CreateDoctorCommand : IRequest<CreateDoctorResponse>
    {
        public CreateDoctorRequest Request { get; set; }

        public CreateDoctorCommand(CreateDoctorRequest request)
        {
            Request = request;
        }
    }
}
