using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Infrastructure.Repositories
{
    public class DepartmentRepository : Repository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(ApplicationDBContext dbContext) : base(dbContext) { }

        public async Task<bool> ExistsByNameAsync(string name, long? excludeDepartmentId = null)
        {
            return await _dbSet.AnyAsync(d =>
                d.Name.ToLower() == name.ToLower() &&
                (excludeDepartmentId == null || d.DepartmentId != excludeDepartmentId));
        }
    }
}
