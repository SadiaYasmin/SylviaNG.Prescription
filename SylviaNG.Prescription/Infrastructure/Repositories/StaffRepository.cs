using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Infrastructure.Repositories
{
    public class StaffRepository : Repository<Staff>, IStaffRepository
    {
        public StaffRepository(ApplicationDBContext dbContext) : base(dbContext) { }

        public async Task<Staff?> GetByUserIdAsync(long userId)
        {
            return await _dbSet.FirstOrDefaultAsync(s => s.UserId == userId);
        }
    }
}
