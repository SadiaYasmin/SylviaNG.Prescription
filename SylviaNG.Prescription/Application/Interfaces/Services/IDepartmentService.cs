using SylviaNG.Prescription.Application.Features.Departments.Models;

namespace SylviaNG.Prescription.Application.Interfaces.Services
{
    public interface IDepartmentService
    {
        Task<long> CreateAsync(DepartmentCreateRequest request);
        Task UpdateAsync(long id, DepartmentUpdateRequest request);
        Task DeactivateAsync(long id);
        Task<List<DepartmentResponse>> GetAllAsync();
    }
}
