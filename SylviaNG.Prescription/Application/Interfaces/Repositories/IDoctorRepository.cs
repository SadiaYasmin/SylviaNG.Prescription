using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Interfaces.Repositories
{
    public interface IDoctorRepository : IRepository<Doctor>
    {
        Task<Doctor?> GetByUserIdAsync(long userId);
        Task<bool> ExistsByLicenseNumberAsync(string licenseNumber, long? excludeDoctorId = null);
    }
}
