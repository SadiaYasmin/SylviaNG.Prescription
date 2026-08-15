using SylviaNG.Prescription.Application.Features.HospitalSettings.Models;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Application.Mappings
{
    public static class HospitalSettingsMappings
    {
        public static HospitalSettingsResponse ToResponse(this HospitalSettings settings)
        {
            return new HospitalSettingsResponse
            {
                HospitalSettingsId = settings.HospitalSettingsId,
                Name = settings.Name,
                LogoUrl = settings.LogoUrl,
                Address = settings.Address,
                Phone = settings.Phone,
                EmergencyNumber = settings.EmergencyNumber,
                Email = settings.Email,
                Website = settings.Website,
                Slogan = settings.Slogan,
                SloganBn = settings.SloganBn,
                LicenseNumber = settings.LicenseNumber,
                SealUrl = settings.SealUrl
            };
        }
    }
}
