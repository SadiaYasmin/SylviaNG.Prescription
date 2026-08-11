using MediatR;
using SylviaNG.Prescription.Application.Features.HospitalSettings.Models;

namespace SylviaNG.Prescription.Application.Features.HospitalSettings.Queries.GetHospitalSettings
{
    public class GetHospitalSettingsQuery : IRequest<HospitalSettingsResponse>
    {
    }
}
