using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Infrastructure.Repositories
{
    public class DoctorRepository : Repository<Doctor>, IDoctorRepository
    {
        public DoctorRepository(ApplicationDBContext dbContext) : base(dbContext) { }

        public async Task<Doctor?> GetByUserIdAsync(long userId)
        {
            return await _dbSet.FirstOrDefaultAsync(d => d.UserId == userId);
        }

        public async Task<bool> ExistsByLicenseNumberAsync(string licenseNumber, long? excludeDoctorId = null)
        {
            return await _dbSet.AnyAsync(d =>
                d.LicenseNumber == licenseNumber &&
                (excludeDoctorId == null || d.DoctorId != excludeDoctorId));
        }
    }
}
