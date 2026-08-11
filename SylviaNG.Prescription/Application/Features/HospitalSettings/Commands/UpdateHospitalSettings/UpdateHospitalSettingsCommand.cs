using MediatR;
using SylviaNG.Prescription.Application.Features.HospitalSettings.Models;

namespace SylviaNG.Prescription.Application.Features.HospitalSettings.Commands.UpdateHospitalSettings
{
    public class UpdateHospitalSettingsCommand : IRequest<HospitalSettingsResponse>
    {
        public UpdateHospitalSettingsRequest Request { get; set; }

        public UpdateHospitalSettingsCommand(UpdateHospitalSettingsRequest request)
        {
            Request = request;
        }
    }
}
