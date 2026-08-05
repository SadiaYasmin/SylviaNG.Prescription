using MediatR;
using SylviaNG.Prescription.Application.Features.Doctors.Models;

namespace SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctor
{
    public class UpdateDoctorCommand : IRequest<DoctorSummaryResponse>
    {
        public long DoctorId { get; set; }
        public UpdateDoctorRequest Request { get; set; }

        public UpdateDoctorCommand(long doctorId, UpdateDoctorRequest request)
        {
            DoctorId = doctorId;
            Request = request;
        }
    }
}
