using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Interfaces.Repositories
{
    public interface IDepartmentRepository : IRepository<Department>
    {
        Task<bool> ExistsByNameAsync(string name, long? excludeDepartmentId = null);
    }
}
